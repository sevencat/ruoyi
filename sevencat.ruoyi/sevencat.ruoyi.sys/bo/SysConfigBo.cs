namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 参数配置业务对象 sys_config
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysConfig.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
public class SysConfigBo
{
	/// <summary>
	/// 参数主键
	/// </summary>
	public long? ConfigId { get; set; }

	/// <summary>
	/// 参数名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "参数名称不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "参数名称不能超过{max}个字符")
	public string ConfigName { get; set; }

	/// <summary>
	/// 参数键名
	/// </summary>
	// Java 原注解 @NotBlank(message = "参数键名不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "参数键名长度不能超过{max}个字符")
	public string ConfigKey { get; set; }

	/// <summary>
	/// 参数键值
	/// </summary>
	// Java 原注解 @NotBlank(message = "参数键值不能为空")
	// Java 原注解 @Size(min = 0, max = 500, message = "参数键值长度不能超过{max}个字符")
	public string ConfigValue { get; set; }

	/// <summary>
	/// 系统内置（Y是 N否）
	/// </summary>
	public string ConfigType { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}