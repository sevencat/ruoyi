using MiniExcelLibs.Attributes;

namespace sevencat.ruoyi.demo.vo;

/// <summary>
/// 测试单表导入对象 test_demo
/// </summary>
/// Java 原注解 @AutoMapper(target = TestDemo.class)：C# 端无对应映射框架注解，导入时由 Service 手工转换
public class TestDemoImportVo
{
	/// <summary>
	/// 部门id
	/// </summary>
	// Java 原注解 @NotNull(message = "部门id不能为空")
	[ExcelColumn(Name = "部门id")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户id
	/// </summary>
	// Java 原注解 @NotNull(message = "用户id不能为空")
	[ExcelColumn(Name = "用户id")]
	public long? UserId { get; set; }

	/// <summary>
	/// 排序号
	/// </summary>
	// Java 原类型为 Long，表结构 order_num 为 INT，这里按 int? 接收
	// Java 原注解 @NotNull(message = "排序号不能为空")
	[ExcelColumn(Name = "排序号")]
	public int? OrderNum { get; set; }

	/// <summary>
	/// key键
	/// </summary>
	// Java 原注解 @NotBlank(message = "key键不能为空")
	[ExcelColumn(Name = "key键")]
	public string TestKey { get; set; }

	/// <summary>
	/// 值
	/// </summary>
	// Java 原注解 @NotBlank(message = "值不能为空")
	[ExcelColumn(Name = "值")]
	public string Value { get; set; }
}
