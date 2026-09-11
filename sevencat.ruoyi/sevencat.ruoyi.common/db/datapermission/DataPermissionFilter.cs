using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Util;
using FreeSql;
using FreeSql.Internal;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.db.datapermission;

/// <summary>
/// 数据权限的 Freesql AOP 实现
/// （对应 Java 的 <c>PlusDataPermissionHandler</c> + <c>PlusDataPermissionInterceptor</c> + <c>DataPermissionAspect</c>）。
/// </summary>
/// <remarks>
/// <para>Java 端链路：<c>@DataPermission</c> 注解（Mapper 方法）+ MyBatis 拦截器 + JSqlParser 改写 SQL 的 WHERE。</para>
/// <para>C# 端没有 Mapper 与注解拦截点，故改为在 Freesql AOP上体现，思路如下：</para>
/// <list type="number">
/// <item>
/// 启动时为每个标注了 <see cref="DataScopeAttribute"/> 的实体注册一个全局过滤器占位表达式
/// <c>x =&gt; DataPermissionHelper.Filter(x)</c>；并通过 <c>GlobalFilter.ApplyIf</c> 的 condition
/// 表达「已登录 + 非超级管理员 + 非忽略上下文」时才启用（对应 Java 拦截前的前置判断）。
/// </item>
/// <item>
/// 在 <c>Aop.ParseExpression</c> 中命中该占位方法调用时，按当前登录用户的角色数据范围
/// （<c>RoleDTO.DataScope</c>）构建过滤表达式，再交给 <c>e.FreeParse</c> 生成 SQL 片段，
/// 从而自动带上正确的列名与表别名（对应 Java 的 JSqlParser 改写）。
/// </item>
/// <item>
/// 构建条件时查询 sys_role_dept / sys_dept 会经过 <see cref="DataPermissionHelper.Ignore"/>，
/// 避免递归触发数据权限（与 Java 一致）。
/// </item>
/// </list>
/// <para>
/// 与 Java 的差异：Java 会区分 select 与 update/delete（后者用 AND 连接各角色条件，语义更严格）；
/// C# 端 AOP 层拿不到 CURD 类型，故统一按 OR 连接（查询语义）。
/// 如需 update/delete 更严格，可在 <c>CurdBefore</c> 中结合 <c>e.CurdType</c> 传递上下文后扩展。
/// </para>
/// </remarks>
public static class DataPermissionFilter
{
	/// <summary>
	/// 全局过滤器名称
	/// </summary>
	private const string FilterName = "sevencat:dataScope";

	/// <summary>
	/// 占位方法 <see cref="DataPermissionHelper.Filter{T}"/> 的定义
	/// </summary>
	private static readonly MethodInfo FilterMethod = typeof(DataPermissionHelper)
		.GetMethod(nameof(DataPermissionHelper.Filter));

	private static readonly ConcurrentDictionary<Type, DataScopeAttribute> AttributeCache = new();

	/// <summary>
	/// 数据权限服务（延迟解析，避免启动期容器未就绪；未注册时按无结果处理）
	/// </summary>
	private static readonly Lazy<IDataScopeService> ScopeService = IocFactory.CreateLazy<IDataScopeService>();

	/// <summary>
	/// 装配数据权限 AOP（由 <c>FSqlAop.SetupFreesql</c> 调用）
	/// </summary>
	/// <param name="fsql">Freesql 实例</param>
	public static void Setup(IFreeSql fsql)
	{
		foreach (var entityType in DiscoverEntities())
		{
			RegisterGlobalFilter(fsql, entityType);
		}

		fsql.Aop.ParseExpression += (_, e) =>
		{
			// 只处理占位方法调用，其它表达式节点保持 Freesql 默认解析
			if (e.Expression is not MethodCallExpression call
				|| call.Method.DeclaringType != typeof(DataPermissionHelper)
				|| call.Method.Name != nameof(DataPermissionHelper.Filter))
			{
				return;
			}

			var entityType = call.Method.GetGenericArguments()[0];
			var predicate = BuildPredicate(entityType, call.Arguments[0]);

			// 交给 Freesql 解析成 SQL，自动带上正确的列名 / 表别名
			e.Result = predicate == null ? "1 = 1" : e.FreeParse(predicate);
		};
	}

	/// <summary>
	/// 为指定实体注册全局过滤器占位表达式
	/// </summary>
	/// <param name="fsql">Freesql 实例</param>
	/// <param name="entityType">实体类型</param>
	private static void RegisterGlobalFilter(IFreeSql fsql, Type entityType)
	{
		// x => DataPermissionHelper.Filter(x)
		var parameter = Expression.Parameter(entityType, "x");
		var body = Expression.Call(FilterMethod.MakeGenericMethod(entityType), parameter);
		var where = Expression.Lambda(
			typeof(Func<,>).MakeGenericType(entityType, typeof(bool)), body, parameter);

		// () => DataPermissionHelper.ShouldFilter<T>()
		var conditionMethod = typeof(DataPermissionHelper)
			.GetMethod(nameof(DataPermissionHelper.ShouldFilter))
			.MakeGenericMethod(entityType);
		var condition = Expression.Lambda<Func<bool>>(Expression.Call(conditionMethod)).Compile();

		// GlobalFilter.ApplyIf<T>(name, condition, where, before)
		var applyIf = typeof(GlobalFilter)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Single(m => m.Name == nameof(GlobalFilter.ApplyIf) && m.GetParameters().Length == 4)
			.MakeGenericMethod(entityType);

		applyIf.Invoke(fsql.GlobalFilter, [FilterName, condition, where, true]);
	}

	/// <summary>
	/// 扫描已加载程序集中带 <see cref="DataScopeAttribute"/> 的实体
	/// </summary>
	/// <returns>实体类型集合</returns>
	private static IEnumerable<Type> DiscoverEntities()
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.IsDynamic)
			{
				continue;
			}

			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types.Where(t => t != null).ToArray();
			}
			catch
			{
				continue;
			}

			foreach (var type in types)
			{
				if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
				{
					continue;
				}

				if (GetAttribute(type) != null)
				{
					yield return type;
				}
			}
		}
	}

	/// <summary>
	/// 获取实体上的数据权限列映射（带缓存）
	/// </summary>
	/// <param name="entityType">实体类型</param>
	/// <returns>映射特性；无则返回 null</returns>
	private static DataScopeAttribute GetAttribute(Type entityType)
	{
		if (AttributeCache.TryGetValue(entityType, out var cached))
		{
			return cached;
		}

		DataScopeAttribute attribute;
		try
		{
			attribute = entityType.GetCustomAttribute<DataScopeAttribute>(true);
		}
		catch
		{
			attribute = null;
		}

		if (attribute != null)
		{
			AttributeCache[entityType] = attribute;
		}

		return attribute;
	}

	/// <summary>
	/// 构建数据权限谓词（对应 Java 的 <c>buildDataFilter</c>）
	/// </summary>
	/// <param name="entityType">实体类型</param>
	/// <param name="entity">Freesql 解析上下文中的实体表达式（占位方法的实参）</param>
	/// <returns>过滤表达式；无需过滤时返回 null</returns>
	private static Expression BuildPredicate(Type entityType, Expression entity)
	{
		var attribute = GetAttribute(entityType);
		if (attribute == null)
		{
			return null;
		}

		var user = DataPermissionHelper.CurrentUser();
		if (user == null || user.IsSuperAdmin())
		{
			return null;
		}

		if (user.Roles is not { Count: > 0 })
		{
			return null;
		}

		Expression result = null;
		foreach (var role in user.Roles)
		{
			if (role.RoleId == null)
			{
				continue;
			}

			var type = DataScopeTypeHelper.FindCode(role.DataScope);
			if (type == null)
			{
				continue;
			}

			// 任一角色拥有全部数据权限 -> 直接放行（对应 Java 的 DataScopeType.ALL 短路）
			if (type == DataScopeType.All)
			{
				return null;
			}

			var condition = BuildRolePredicate(entityType, entity, attribute, user, role, type.Value);
			if (condition == null)
			{
				continue;
			}

			// 查询语义：多个角色条件取 OR（对应 Java isSelect=true 时的 joinStr）
			result = result == null ? condition : Expression.OrElse(result, condition);
		}

		return result;
	}

	/// <summary>
	/// 构建单个角色的数据权限条件
	/// </summary>
	/// <param name="entityType">实体类型</param>
	/// <param name="entity">实体表达式</param>
	/// <param name="attribute">列映射</param>
	/// <param name="user">当前登录用户</param>
	/// <param name="role">角色</param>
	/// <param name="type">数据范围类型</param>
	/// <returns>条件表达式；无法构建时返回 null</returns>
	private static Expression BuildRolePredicate(
		Type entityType, Expression entity, DataScopeAttribute attribute,
		LoginUser user, RoleDTO role, DataScopeType type)
	{
		switch (type)
		{
			case DataScopeType.Custom:
			{
				var deptColumn = ResolveColumn(entityType, entity, attribute.DeptColumn);
				if (deptColumn == null)
				{
					return False();
				}

				var deptIds = DataPermissionHelper.Ignore(
					() => ScopeServiceOrNull()?.GetRoleCustom(role.RoleId.Value) ?? []);
				return deptIds.Count == 0 ? False() : In(deptColumn, deptIds);
			}

			case DataScopeType.Dept:
			{
				var deptColumn = ResolveColumn(entityType, entity, attribute.DeptColumn);
				if (deptColumn == null || user.DeptId == null)
				{
					return False();
				}

				return Eq(deptColumn, user.DeptId.Value);
			}

			case DataScopeType.DeptAndChild:
			{
				var deptColumn = ResolveColumn(entityType, entity, attribute.DeptColumn);
				if (deptColumn == null || user.DeptId == null)
				{
					return False();
				}

				var deptIds = DataPermissionHelper.Ignore(
					() => ScopeServiceOrNull()?.GetDeptAndChild(user.DeptId.Value) ?? []);
				return deptIds.Count == 0 ? False() : In(deptColumn, deptIds);
			}

			case DataScopeType.Self:
			{
				var userColumn = ResolveColumn(entityType, entity, attribute.UserColumn);
				if (userColumn == null || user.UserId == null)
				{
					return False();
				}

				return Eq(userColumn, user.UserId.Value);
			}

			case DataScopeType.DeptAndChildOrSelf:
			{
				var deptColumn = ResolveColumn(entityType, entity, attribute.DeptColumn);
				var userColumn = ResolveColumn(entityType, entity, attribute.UserColumn);

				Expression deptCondition = False();
				Expression selfCondition = False();

				if (deptColumn != null && user.DeptId != null)
				{
					var deptIds = DataPermissionHelper.Ignore(
						() => ScopeServiceOrNull()?.GetDeptAndChild(user.DeptId.Value) ?? []);
					deptCondition = deptIds.Count == 0 ? False() : In(deptColumn, deptIds);
				}

				if (userColumn != null && user.UserId != null)
				{
					selfCondition = Eq(userColumn, user.UserId.Value);
				}

				return Expression.OrElse(deptCondition, selfCondition);
			}

			default:
				return null;
		}
	}

	/// <summary>
	/// 将「属性名」解析为实体的列表达式；不存在或类型非整型时返回 null
	/// </summary>
	/// <param name="entityType">实体类型</param>
	/// <param name="entity">实体表达式</param>
	/// <param name="propertyName">属性名</param>
	/// <returns>列表达式；无效时返回 null</returns>
	private static Expression ResolveColumn(Type entityType, Expression entity, string propertyName)
	{
		if (string.IsNullOrWhiteSpace(propertyName))
		{
			return null;
		}

		var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		if (property == null
			|| (property.PropertyType != typeof(long) && property.PropertyType != typeof(long?)))
		{
			return null;
		}

		return Expression.Property(entity, property);
	}

	/// <summary>
	/// 构建 <c>列 IN (...)</c> 表达式（<c>IN</c> 列表为空的情况由调用方先行转为 <see cref="False"/>）
	/// </summary>
	/// <param name="property">列表达式</param>
	/// <param name="values">取值集合</param>
	/// <returns>Contains 调用表达式</returns>
	private static Expression In(Expression property, List<long> values)
	{
		var elementType = property.Type;
		var listType = typeof(List<>).MakeGenericType(elementType);
		var list = (IList)Activator.CreateInstance(listType);
		foreach (var value in values)
		{
			list.Add(value);
		}

		var constant = Expression.Constant(list, listType);
		var contains = listType.GetMethod(nameof(List<long>.Contains), [elementType]);
		return Expression.Call(constant, contains, property);
	}

	/// <summary>
	/// 构建 <c>列 = 值</c> 表达式（自动适配 long / long? 列类型）
	/// </summary>
	/// <param name="property">列表达式</param>
	/// <param name="value">比较值</param>
	/// <returns>相等表达式</returns>
	private static Expression Eq(Expression property, long value)
	{
		Expression right = property.Type == typeof(long)
			? Expression.Constant(value, typeof(long))
			: Expression.Constant((long?)value, property.Type);
		return Expression.Equal(property, right);
	}

	/// <summary>
	/// 恒假表达式（对应 Java 模板中的兜底 SQL <c>1 = 0</c>）
	/// </summary>
	/// <returns>恒假表达式</returns>
	private static Expression False() => Expression.Equal(Expression.Constant(1), Expression.Constant(0));

	/// <summary>
	/// 解析数据权限服务（未注册时返回 null）
	/// </summary>
	/// <returns>数据权限服务</returns>
	private static IDataScopeService ScopeServiceOrNull()
	{
		try
		{
			return ScopeService.Value;
		}
		catch
		{
			return null;
		}
	}
}
