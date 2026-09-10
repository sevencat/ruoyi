namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 岗位信息业务对象 sys_post
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysPost.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@NotNull/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysPostBo
{
	/// <summary>
	/// 岗位ID
	/// </summary>
	public long? PostId { get; set; }

	/// <summary>
	/// 部门id（单部门）
	/// </summary>
	// Java 原注解 @NotNull(message = "部门id不能为空")
	public long? DeptId { get; set; }

	/// <summary>
	/// 归属部门id（部门树）
	/// </summary>
	public long? BelongDeptId { get; set; }

	/// <summary>
	/// 岗位编码
	/// </summary>
	// Java 原注解 @NotBlank(message = "岗位编码不能为空")
	// Java 原注解 @Size(min = 0, max = 64, message = "岗位编码长度不能超过{max}个字符")
	public string PostCode { get; set; }

	/// <summary>
	/// 岗位名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "岗位名称不能为空")
	// Java 原注解 @Size(min = 0, max = 50, message = "岗位名称长度不能超过{max}个字符")
	public string PostName { get; set; }

	/// <summary>
	/// 岗位类别编码
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 100, message = "类别编码长度不能超过{max}个字符")
	public string PostCategory { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	// Java 原注解 @NotNull(message = "显示顺序不能为空")
	public int? PostSort { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}
