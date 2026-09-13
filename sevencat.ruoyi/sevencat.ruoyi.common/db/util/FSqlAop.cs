using System.Collections.Concurrent;
using System.Reflection;
using sevencat.ruoyi.common.db;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.db.datapermission;
using sevencat.ruoyi.common.entity.db;
using sevencat.ruoyi.common.security;

namespace sevencat.ruoyi.common.db.util;

public class FSqlAop
{
	/// <summary>
	/// 实体类型 -&gt; 该实体上带 <see cref="SnowflakeAttribute"/> 的属性（没有则为 null）
	/// </summary>
	/// <remarks>
	/// AuditValue 是**按属性逐个触发**的，每次都要判断"当前这一列是不是雪花列"；
	/// 这里按类型缓存一次反射结果，避免每行 × 每列都做一次特性查找。
	/// <see cref="SnowflakeAttribute"/> 的 AttributeUsage 只允许标注属性，所以只需扫属性、不用扫字段。
	/// </remarks>
	private static readonly ConcurrentDictionary<Type, PropertyInfo> SnowflakeProps = new();

	/// <summary>
	/// 取实体上带 <see cref="SnowflakeAttribute"/> 的属性
	/// </summary>
	/// <param name="type">实体类型</param>
	/// <returns>雪花属性；入参为空或该类型未标注时返回 null</returns>
	public static PropertyInfo GetSnowflakeProperty(Type type)
	{
		if (type == null)
		{
			return null;
		}

		return SnowflakeProps.GetOrAdd(type, static t => t
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.FirstOrDefault(p => p.IsDefined(typeof(SnowflakeAttribute), inherit: true)));
	}

	public static void SetupFreesql(IFreeSql fsql)
	{
		fsql.Aop.AuditValue += (s, e) => { Handle(e); };

		// 装配数据权限 AOP（对应 Java 的 PlusDataPermissionInterceptor）
		DataPermissionFilter.Setup(fsql);
	}

	const long DEFAULT_USER_ID = -1L;

	/// <summary>
	/// AuditValue 回调：填充雪花主键与审计字段（每个实体只处理一次）
	/// </summary>
	/// <param name="e">审计事件参数</param>
	/// <remarks>
	/// FreeSql 的 AuditValue 是"读一列 → 触发一次事件"的逐列即时读取（见 v3.5.311
	/// InsertProvider/UpdateProvider.AuditDataValue），而真正写入 SQL 的值是在生成语句时
	/// 重新从实体属性读取的（col.GetDbValue(d)）。
	/// 所以本行任意一列触发进来时，把整行要填的值直接改到实体上，再置
	/// <c>e.ObjectAuditBreak = true</c> 中断该实体剩余列的审计即可：
	/// 与列的前后顺序无关，[Snowflake] 主键也不会漏填。
	/// 注意 ObjectAuditBreak 只跳出"当前实体"的列循环，批量插入时其余实体照常审计。
	/// </remarks>
	public static void Handle(FreeSql.Aop.AuditValueEventArgs e)
	{
		if (e.Object is not TBaseEntity entity)
		{
			return;
		}

		var lu = LoginHelper.FastGetLoginUser();
		var uid = lu?.UserId ?? DEFAULT_USER_ID;

		if (e.AuditValueType == FreeSql.Aop.AuditValueType.Insert)
		{
			// 雪花主键：只对 long 的默认值（0）填充，防止手动赋值被覆盖；属性查找按类型缓存
			var snow = GetSnowflakeProperty(entity.GetType());
			if (snow != null && snow.PropertyType == typeof(long) && (long)snow.GetValue(entity) == 0L)
			{
				// 必须复用容器中的 IIdGen 单例：SnowflakeIdWorker 的 sequence / lastTimestamp 是实例字段，
				// 两个同 workerId+datacenterId 的实例在同一毫秒内会各自从 sequence=0 开始，生成完全相同的 ID
				snow.SetValue(entity, IdGenUtil.NextId());
			}

			entity.CreateBy = uid;
			entity.CreateDept = lu?.DeptId;
			entity.CreateTime = DateTime.Now;
		}
		else if (e.AuditValueType == FreeSql.Aop.AuditValueType.Update)
		{
			entity.UpdateBy = uid;
			entity.UpdateTime = DateTime.Now;
		}

		// 本行已处理完：该实体剩余的列不再触发本回调
		e.ObjectAuditBreak = true;
	}
}