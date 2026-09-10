namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 部门业务对象 sys_dept
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysDept.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@Size/@NotNull/@Email 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysDeptBo
{
	/// <summary>
	/// 部门id
	/// </summary>
	public long? DeptId { get; set; }

	/// <summary>
	/// 父部门ID
	/// </summary>
	public long? ParentId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "部门名称不能为空")
	// Java 原注解 @Size(min = 0, max = 30, message = "部门名称长度不能超过{max}个字符")
	public string DeptName { get; set; }

	/// <summary>
	/// 部门类别编码
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 100, message = "部门类别编码长度不能超过{max}个字符")
	public string DeptCategory { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	// Java 原注解 @NotNull(message = "显示顺序不能为空")
	public int? OrderNum { get; set; }

	/// <summary>
	/// 负责人
	/// </summary>
	public long? Leader { get; set; }

	/// <summary>
	/// 联系电话
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 11, message = "联系电话长度不能超过{max}个字符")
	public string Phone { get; set; }

	/// <summary>
	/// 邮箱
	/// </summary>
	// Java 原注解 @Email(message = "邮箱格式不正确")
	// Java 原注解 @Size(min = 0, max = 50, message = "邮箱长度不能超过{max}个字符")
	public string Email { get; set; }

	/// <summary>
	/// 部门状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 归属部门id（部门树）
	/// </summary>
	public long? BelongDeptId { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}
