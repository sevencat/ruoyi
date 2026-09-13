using Autofac.Annotation;
using Microsoft.Extensions.Configuration;
using sevencat.ruoyi.common.db;
using sevencat.ruoyi.common.db.util;

namespace sevencat.ruoyi.config;

/// <summary>
/// 雪花 ID 生成器配置
/// </summary>
/// <remarks>
/// workerId / datacenterId 必须全局唯一（多实例、多节点部署都不能重复），否则会生成重复 ID。
/// 默认值 1 只适用于「单机单实例」，多实例部署请在 appsettings.json 中区分，例如：
/// <code>"snowflake": { "workerId": 2, "datacenterId": 1 }</code>
/// </remarks>
[AutoConfiguration]
public class IdGenConfig
{
	/// <summary>
	/// 注册全局唯一的 ID 生成器（容器单例，雪花主键填充与业务代码共用同一实例）
	/// </summary>
	/// <param name="configuration">应用配置</param>
	/// <returns>ID 生成器</returns>
	[Bean]
	public IIdGen CreateIdGen(IConfiguration configuration)
	{
		var workerId = configuration.GetValue<long>("snowflake:workerId", 1L);
		var datacenterId = configuration.GetValue<long>("snowflake:datacenterId", 1L);
		return new SnowflakeIdWorker(workerId, datacenterId);
	}
}
