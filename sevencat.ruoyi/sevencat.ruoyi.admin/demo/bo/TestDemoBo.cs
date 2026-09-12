namespace sevencat.ruoyi.demo.bo;

/// <summary>
/// 测试单表业务对象 test_demo
/// </summary>
/// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = TestDemo.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotNull/@NotBlank 的 AddGroup/EditGroup 分组）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
public class TestDemoBo
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 原注解 @NotNull(message = "主键不能为空", groups = {EditGroup.class})
	public long? Id { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	// Java 原注解 @NotNull(message = "部门id不能为空", groups = {AddGroup.class, EditGroup.class})
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户id
	/// </summary>
	// Java 原注解 @NotNull(message = "用户id不能为空", groups = {AddGroup.class, EditGroup.class})
	public long? UserId { get; set; }

	/// <summary>
	/// 排序号
	/// </summary>
	// Java 原注解 @NotNull(message = "排序号不能为空", groups = {AddGroup.class, EditGroup.class})
	public int? OrderNum { get; set; }

	/// <summary>
	/// key键
	/// </summary>
	// Java 原注解 @NotBlank(message = "key键不能为空", groups = {AddGroup.class, EditGroup.class})
	public string TestKey { get; set; }

	/// <summary>
	/// 值
	/// </summary>
	// Java 原注解 @NotBlank(message = "值不能为空", groups = {AddGroup.class, EditGroup.class})
	public string Value { get; set; }

	/// <summary>
	/// 版本
	/// </summary>
	// Java 原类型为 Long，表结构 version 为 INT，这里按 int? 接收
	public int? Version { get; set; }
}
