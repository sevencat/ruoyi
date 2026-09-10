namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 对象存储配置业务对象 sys_oss_config
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysOssConfig.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@NotNull/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysOssConfigBo
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 原注解 @NotNull(message = "主键不能为空", groups = {EditGroup.class})
	public long? OssConfigId { get; set; }

	/// <summary>
	/// 配置key
	/// </summary>
	// Java 原注解 @NotBlank(message = "配置key不能为空", groups = {AddGroup.class, EditGroup.class})
	// Java 原注解 @Size(min = 2, max = 100, message = "configKey长度必须介于{min}和{max} 之间")
	public string ConfigKey { get; set; }

	/// <summary>
	/// accessKey
	/// </summary>
	// Java 原注解 @NotBlank(message = "accessKey不能为空", groups = {AddGroup.class, EditGroup.class})
	// Java 原注解 @Size(min = 2, max = 100, message = "accessKey长度必须介于{min}和{max} 之间")
	public string AccessKey { get; set; }

	/// <summary>
	/// 秘钥
	/// </summary>
	// Java 原注解 @NotBlank(message = "secretKey不能为空", groups = {AddGroup.class, EditGroup.class})
	// Java 原注解 @Size(min = 2, max = 100, message = "secretKey长度必须介于{min}和{max} 之间")
	public string SecretKey { get; set; }

	/// <summary>
	/// 桶名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "桶名称不能为空", groups = {AddGroup.class, EditGroup.class})
	// Java 原注解 @Size(min = 2, max = 100, message = "bucketName长度必须介于{min}和{max}之间")
	public string BucketName { get; set; }

	/// <summary>
	/// 前缀
	/// </summary>
	public string Prefix { get; set; }

	/// <summary>
	/// 访问站点
	/// </summary>
	// Java 原注解 @NotBlank(message = "访问站点不能为空", groups = {AddGroup.class, EditGroup.class})
	// Java 原注解 @Size(min = 2, max = 100, message = "endpoint长度必须介于{min}和{max}之间")
	public string Endpoint { get; set; }

	/// <summary>
	/// 自定义域名
	/// </summary>
	public string DomainUrl { get; set; }

	/// <summary>
	/// 是否https（Y=是,N=否）
	/// </summary>
	public string IsHttps { get; set; }

	/// <summary>
	/// 是否默认（Y=是,N=否）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 域
	/// </summary>
	public string Region { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	public string Ext1 { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 桶权限类型(0private 1public 2custom)
	/// </summary>
	// Java 原注解 @NotBlank(message = "桶权限类型不能为空", groups = {AddGroup.class, EditGroup.class})
	public string AccessPolicy { get; set; }
}
