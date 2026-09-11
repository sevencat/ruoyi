using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.attr;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 系统访问记录表 sys_login_info
/// </summary>
/// <remarks>
/// 表中无 create_by / create_time 等审计字段，故不继承
/// <see cref="sevencat.ruoyi.common.entity.db.TBaseEntity"/>。
/// </remarks>
[Table(Name = "sys_login_info")]
[Index("idx_sys_login_info_s", "Status")]
[Index("idx_sys_login_info_lt", "LoginTime")]
public class TSysLoginInfo
{
	/// <summary>
	/// 访问ID
	/// </summary>
	[Column(Name = "info_id", IsPrimary = true, IsIdentity = true)]
	public long InfoId { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	[Column(Name = "user_name", StringLength = 50, IsNullable = true)]
	public string UserName { get; set; } = string.Empty;

	/// <summary>
	/// 客户端
	/// </summary>
	[Column(Name = "client_key", StringLength = 32, IsNullable = true)]
	public string ClientKey { get; set; } = string.Empty;

	/// <summary>
	/// 设备类型
	/// </summary>
	[Column(Name = "device_type", StringLength = 32, IsNullable = true)]
	public string DeviceType { get; set; } = string.Empty;

	/// <summary>
	/// 登录IP地址
	/// </summary>
	[Column(Name = "ipaddr", StringLength = 128, IsNullable = true)]
	public string Ipaddr { get; set; } = string.Empty;

	/// <summary>
	/// 登录地点
	/// </summary>
	[Column(Name = "login_location", StringLength = 255, IsNullable = true)]
	public string LoginLocation { get; set; } = string.Empty;

	/// <summary>
	/// 浏览器类型
	/// </summary>
	[Column(Name = "browser", StringLength = 50, IsNullable = true)]
	public string Browser { get; set; } = string.Empty;

	/// <summary>
	/// 操作系统
	/// </summary>
	[Column(Name = "os", StringLength = 50, IsNullable = true)]
	public string Os { get; set; } = string.Empty;

	/// <summary>
	/// 登录状态（0正常 1异常）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; } = SystemConstants.NORMAL;

	/// <summary>
	/// 提示消息
	/// </summary>
	[Column(Name = "msg", StringLength = 255, IsNullable = true)]
	public string Msg { get; set; } = string.Empty;

	/// <summary>
	/// 访问时间
	/// </summary>
	[Column(Name = "login_time", IsNullable = true)]
	public DateTime? LoginTime { get; set; }
}