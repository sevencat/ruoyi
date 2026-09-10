using System.Reflection;
using sevencat.ruoyi.common.db.attr;

namespace sevencat.ruoyi.common.db.util;

public class FSqlAop
{
	private static readonly SnowflakeIdWorker _idWorker = new SnowflakeIdWorker(workerId: 1, datacenterId: 1);
	public static void SetupFreesql(IFreeSql fsql)
	{
		fsql.Aop.AuditValue += (s, e) =>
		{
			// 只要是插入或更新操作，且属性拥有 [Snowflake] 特性
			if (e.Property.GetCustomAttribute<SnowflakeAttribute>() != null)
			{
				// 只对 long 类型的默认值（0）进行雪花ID填充，防止手动赋值被覆盖
				if (e.Property.PropertyType == typeof(long) && (long)e.Value == 0L)
				{
					e.Value = _idWorker.NextId();
				}
			}
		};
	}
}