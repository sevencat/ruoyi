using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.security;

namespace sevencat.ruoyi.common.db.datapermission;

/// <summary>
/// 数据权限助手（对应 Java 的 <c>org.dromara.common.mybatis.helper.DataPermissionHelper</c>）。
/// </summary>
public static class DataPermissionHelper
{
	/// <summary>
	/// 忽略数据权限的嵌套深度（对应 Java 基于 ThreadLocal 的 ignore 上下文，此处改用 AsyncLocal）
	/// </summary>
	private static readonly AsyncLocal<int> IgnoreDepth = new();

	/// <summary>
	/// 当前是否处于「忽略数据权限」的上下文
	/// </summary>
	public static bool IsIgnored => IgnoreDepth.Value > 0;

	/// <summary>
	/// 开启一个忽略数据权限的作用域（对应 Java 的 <c>DataPermissionHelper.ignore</c>）。
	/// </summary>
	/// <remarks>
	/// 数据权限构建过程本身会查询 sys_role_dept / sys_dept，
	/// 必须忽略以避免递归触发数据权限造成死循环（Java 亦是如此处理）。
	/// </remarks>
	/// <returns>可释放的作用域</returns>
	public static IDisposable Ignore()
	{
		IgnoreDepth.Value++;
		return new IgnoreScope();
	}

	/// <summary>
	/// 在忽略数据权限的上下文中执行操作
	/// </summary>
	/// <param name="action">待执行的操作</param>
	public static void Ignore(Action action)
	{
		using (Ignore())
		{
			action();
		}
	}

	/// <summary>
	/// 在忽略数据权限的上下文中执行操作并返回结果
	/// </summary>
	/// <typeparam name="T">返回值类型</typeparam>
	/// <param name="func">待执行的操作</param>
	/// <returns>操作结果</returns>
	public static T Ignore<T>(Func<T> func)
	{
		using (Ignore())
		{
			return func();
		}
	}

	/// <summary>
	/// 在忽略数据权限的上下文中执行异步操作
	/// （对应 Java 的 <c>DataPermissionHelper.ignore</c>；C# 端数据访问为异步语义，故需异步版本）
	/// </summary>
	/// <param name="func">待执行的异步操作</param>
	public static async Task IgnoreAsync(Func<Task> func)
	{
		var depth = IgnoreDepth.Value;
		IgnoreDepth.Value = depth + 1;
		try
		{
			await func();
		}
		finally
		{
			IgnoreDepth.Value = depth;
		}
	}

	/// <summary>
	/// 在忽略数据权限的上下文中执行异步操作并返回结果
	/// </summary>
	/// <typeparam name="T">返回值类型</typeparam>
	/// <param name="func">待执行的异步操作</param>
	/// <returns>操作结果</returns>
	public static async Task<T> IgnoreAsync<T>(Func<Task<T>> func)
	{
		var depth = IgnoreDepth.Value;
		IgnoreDepth.Value = depth + 1;
		try
		{
			return await func();
		}
		finally
		{
			IgnoreDepth.Value = depth;
		}
	}

	/// <summary>
	/// 表达式树占位方法：仅用于让 Freesql 在解析表达式时命中 <c>Aop.ParseExpression</c>，
	/// 运行期不会被真正调用，真实的数据权限条件由 <see cref="DataPermissionFilter"/> 替换。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <param name="entity">实体实例</param>
	/// <returns>固定返回 true</returns>
	public static bool Filter<T>(T entity) => true;

	/// <summary>
	/// 判断当前请求是否需要拼接数据权限条件（对应 Java 中「超级管理员不过滤」的短路逻辑）。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <returns>需要过滤返回 true</returns>
	public static bool ShouldFilter<T>()
	{
		if (IsIgnored)
		{
			return false;
		}

		var user = CurrentUser();
		return user != null && !user.IsSuperAdmin() && user.Roles is { Count: > 0 };
	}

	/// <summary>
	/// 获取当前登录用户（未登录或容器未就绪时返回 null）
	/// </summary>
	/// <returns>登录用户</returns>
	public static LoginUser CurrentUser()
	{
		try
		{
			return LoginHelper.FastGetLoginUser();
		}
		catch
		{
			return null;
		}
	}

	private sealed class IgnoreScope : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			IgnoreDepth.Value--;
		}
	}
}
