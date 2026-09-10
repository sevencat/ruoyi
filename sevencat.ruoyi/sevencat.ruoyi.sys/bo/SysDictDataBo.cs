namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 字典数据业务对象 sys_dict_data
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysDictData.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysDictDataBo
{
	/// <summary>
	/// 字典编码
	/// </summary>
	public long? DictCode { get; set; }

	/// <summary>
	/// 字典排序
	/// </summary>
	public int? DictSort { get; set; }

	/// <summary>
	/// 字典标签
	/// </summary>
	// Java 原注解 @NotBlank(message = "字典标签不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "字典标签长度不能超过{max}个字符")
	public string DictLabel { get; set; }

	/// <summary>
	/// 字典键值
	/// </summary>
	// Java 原注解 @NotBlank(message = "字典键值不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "字典键值长度不能超过{max}个字符")
	public string DictValue { get; set; }

	/// <summary>
	/// 字典类型
	/// </summary>
	// Java 原注解 @NotBlank(message = "字典类型不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "字典类型长度不能超过{max}个字符")
	public string DictType { get; set; }

	/// <summary>
	/// 样式属性（其他样式扩展）
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 100, message = "样式属性长度不能超过{max}个字符")
	public string CssClass { get; set; }

	/// <summary>
	/// 表格回显样式
	/// </summary>
	public string ListClass { get; set; }

	/// <summary>
	/// 是否默认（Y是 N否）
	/// </summary>
	public string IsDefault { get; set; }

	/// <summary>
	/// 创建部门
	/// </summary>
	public long? CreateDept { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }
}
