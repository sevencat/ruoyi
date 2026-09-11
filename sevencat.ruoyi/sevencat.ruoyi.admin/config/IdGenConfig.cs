using Autofac.Annotation;
using sevencat.ruoyi.common.db;
using sevencat.ruoyi.common.db.util;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class IdGenConfig
{
	[Bean]
	public IIdGen CreateIdGen()
	{
		var _idWorker = new SnowflakeIdWorker(workerId: 1, datacenterId: 1);
		return _idWorker;
	}
}