using System.Reflection;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.db.datapermission;
using sevencat.ruoyi.common.entity.db;
using sevencat.ruoyi.common.security;

namespace sevencat.ruoyi.common.db.util;

public class FSqlAop
{
	private static readonly SnowflakeIdWorker _idWorker = new SnowflakeIdWorker(workerId: 1, datacenterId: 1);

	public static void SetupFreesql(IFreeSql fsql)
	{
		fsql.Aop.AuditValue += (s, e) => { Handle(e); };

		// 装配数据权限 AOP（对应 Java 的 PlusDataPermissionInterceptor）
		DataPermissionFilter.Setup(fsql);
	}

	const long DEFAULT_USER_ID = -1L;

	public static void Handle(FreeSql.Aop.AuditValueEventArgs e)
	{
		if (e.Object is TBaseEntity entity)
		{
			var lu = LoginHelper.FastGetLoginUser();
			var uid = lu?.UserId ?? DEFAULT_USER_ID;
			if (e.AuditValueType == FreeSql.Aop.AuditValueType.Insert)
			{
				if (e.Property.GetCustomAttribute<SnowflakeAttribute>() != null)
				{
					// 只对 long 类型的默认值（0）进行雪花ID填充，防止手动赋值被覆盖
					if (e.Property.PropertyType == typeof(long) && (long)e.Value == 0L)
					{
						e.Value = _idWorker.NextId();
					}
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
		}

		e.ObjectAuditBreak = true;
	}
}