namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 系统访问记录业务对象 sys_login_info
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysLoginInfo.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysLoginInfoBo
{
	/// <summary>
	/// 访问ID
	/// </summary>
	public long? InfoId { get; set; }

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
	/// 登录地点
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
	/// 登录状态（0成功 1失败）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 提示消息
	/// </summary>
	public string Msg { get; set; }

	/// <summary>
	/// 访问时间
	/// </summary>
	public DateTime? LoginTime { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}
