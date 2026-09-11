using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 系统授权表
/// </summary>
[Table(Name = "sys_client")]
public class TSysClient : TBaseEntity
{
	/// <summary>
	/// id
	/// </summary>
	[Column(Name = "id", IsPrimary = true)]
	[Snowflake]
	public long Id { get; set; }

	/// <summary>
	/// 客户端id
	/// </summary>
	[Column(Name = "client_id", StringLength = 64, IsNullable = true)]
	public string ClientId { get; set; }

	/// <summary>
	/// 客户端key
	/// </summary>
	[Column(Name = "client_key", StringLength = 32, IsNullable = true)]
	public string ClientKey { get; set; }

	/// <summary>
	/// 客户端秘钥
	/// </summary>
	[Column(Name = "client_secret", StringLength = 255, IsNullable = true)]
	public string ClientSecret { get; set; }

	/// <summary>
	/// 授权类型
	/// </summary>
	[Column(Name = "grant_type", StringLength = 255, IsNullable = true)]
	public string GrantType { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	[Column(Name = "device_type", StringLength = 32, IsNullable = true)]
	public string DeviceType { get; set; }

	/// <summary>
	/// 允许访问路径
	/// </summary>
	[Column(Name = "access_path", StringLength = 2000, IsNullable = true)]
	public string AccessPath { get; set; }

	/// <summary>
	/// IP白名单
	/// </summary>
	[Column(Name = "ip_whitelist", StringLength = 1000, IsNullable = true)]
	public string IpWhitelist { get; set; }

	/// <summary>
	/// token活跃超时时间
	/// </summary>
	[Column(Name = "active_timeout", IsNullable = true)]
	public int? ActiveTimeout { get; set; }

	/// <summary>
	/// token固定超时
	/// </summary>
	[Column(Name = "timeout", IsNullable = true)]
	public int? Timeout { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; }

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", StringLength = 1, IsNullable = true)]
	public string DelFlag { get; set; } = "0";
}
