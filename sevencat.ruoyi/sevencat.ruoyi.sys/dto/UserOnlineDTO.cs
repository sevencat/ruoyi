namespace sevencat.ruoyi.sys.dto;

/// <summary>
/// 当前在线会话信息对象
/// </summary>
public class UserOnlineDTO
{
	/// <summary>
	/// 会话编号
	/// </summary>
	public string TokenId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	public string DeptName { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	public string UserName { get; set; }

	/// <summary>
	/// 客户端
	/// </summary>
	public string ClientKey { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	public string DeviceType { get; set; }

	/// <summary>
	/// 登录IP地址
	/// </summary>
	public string Ipaddr { get; set; }

	/// <summary>
	/// 登录地址
	/// </summary>
	public string LoginLocation { get; set; }

	/// <summary>
	/// 浏览器类型
	/// </summary>
	public string Browser { get; set; }

	/// <summary>
	/// 操作系统
	/// </summary>
	public string Os { get; set; }

	/// <summary>
	/// 登录时间
	/// </summary>
	public long? LoginTime { get; set; }
}
