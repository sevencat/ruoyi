namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 授权管理业务对象 sys_client
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysClient.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotNull/@NotBlank 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysClientBo
{
	/// <summary>
	/// id
	/// </summary>
	// Java 原注解 @NotNull(message = "id不能为空", groups = {EditGroup.class})
	public long? Id { get; set; }

	/// <summary>
	/// 客户端id
	/// </summary>
	public string ClientId { get; set; }

	/// <summary>
	/// 客户端key
	/// </summary>
	// Java 原注解 @NotBlank(message = "客户端key不能为空", groups = {AddGroup.class, EditGroup.class})
	public string ClientKey { get; set; }

	/// <summary>
	/// 客户端秘钥
	/// </summary>
	// Java 原注解 @NotBlank(message = "客户端秘钥不能为空", groups = {AddGroup.class, EditGroup.class})
	public string ClientSecret { get; set; }

	/// <summary>
	/// 授权类型
	/// </summary>
	// Java 原注解 @NotNull(message = "授权类型不能为空", groups = {AddGroup.class, EditGroup.class})
	public List<string> GrantTypeList { get; set; }

	/// <summary>
	/// 授权类型
	/// </summary>
	public string GrantType { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	public string DeviceType { get; set; }

	/// <summary>
	/// 允许访问路径
	/// </summary>
	public string AccessPath { get; set; }

	/// <summary>
	/// 允许访问路径列表
	/// </summary>
	public List<string> AccessPathList { get; set; }

	/// <summary>
	/// IP白名单
	/// </summary>
	public string IpWhitelist { get; set; }

	/// <summary>
	/// IP白名单列表
	/// </summary>
	public List<string> IpWhitelistList { get; set; }

	/// <summary>
	/// token活跃超时时间
	/// </summary>
	public long? ActiveTimeout { get; set; }

	/// <summary>
	/// token固定超时时间
	/// </summary>
	public long? Timeout { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }
}
