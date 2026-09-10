namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 字典类型业务对象 sys_dict_type
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysDictType.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@Size/@Pattern 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysDictTypeBo
{
	/// <summary>
	/// 字典主键
	/// </summary>
	public long? DictId { get; set; }

	/// <summary>
	/// 字典名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "字典名称不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "字典类型名称长度不能超过{max}个字符")
	public string DictName { get; set; }

	/// <summary>
	/// 字典类型
	/// </summary>
	// Java 原注解 @NotBlank(message = "字典类型不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "字典类型类型长度不能超过{max}个字符")
	// Java 原注解 @Pattern(regexp = RegexConstants.DICTIONARY_TYPE, message = "字典类型必须以字母开头，且只能为（小写字母，数字，下滑线）")
	public string DictType { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }
}
