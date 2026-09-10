using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 参数配置表
/// </summary>
[Table(Name = "sys_config")]
public class TSysConfig : TBaseEntity
{
	/// <summary>
	/// 参数主键
	/// </summary>
	[Column(Name = "config_id", IsPrimary = true)]
	[Snowflake]
	public long ConfigId { get; set; }

	/// <summary>
	/// 参数名称
	/// </summary>
	[Column(Name = "config_name", StringLength = 100, IsNullable = true)]
	public string ConfigName { get; set; } = string.Empty;

	/// <summary>
	/// 参数键名
	/// </summary>
	[Column(Name = "config_key", StringLength = 100, IsNullable = true)]
	public string ConfigKey { get; set; } = string.Empty;

	/// <summary>
	/// 参数键值
	/// </summary>
	[Column(Name = "config_value", StringLength = 500, IsNullable = true)]
	public string ConfigValue { get; set; } = string.Empty;

	/// <summary>
	/// 系统内置（Y是 N否）
	/// </summary>
	[Column(Name = "config_type", StringLength = 1, IsNullable = true)]
	public string ConfigType { get; set; } = "N";

	/// <summary>
	/// 备注
	/// </summary>
	[Column(Name = "remark", StringLength = 500, IsNullable = true, Position = -1)]
	public string Remark { get; set; }
}