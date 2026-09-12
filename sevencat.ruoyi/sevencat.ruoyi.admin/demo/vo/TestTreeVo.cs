using MiniExcelLibs.Attributes;

namespace sevencat.ruoyi.demo.vo;

/// <summary>
/// 测试树表视图对象 test_tree
/// </summary>
/// Java 原注解 @AutoMapper(target = TestTree.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class TestTreeVo
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 端无 @ExcelProperty，被类级 @ExcelIgnoreUnannotated 排除，这里用 [ExcelIgnore] 等价表达
	[ExcelIgnore]
	public long? Id { get; set; }

	/// <summary>
	/// 父id
	/// </summary>
	[ExcelColumn(Name = "父id")]
	public long? ParentId { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	[ExcelColumn(Name = "部门id")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户id
	/// </summary>
	[ExcelColumn(Name = "用户id")]
	public long? UserId { get; set; }

	/// <summary>
	/// 树节点名
	/// </summary>
	[ExcelColumn(Name = "树节点名")]
	public string TreeName { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	// Java 原字段为 LocalDateTime，C# 端对应 DateTime?
	[ExcelColumn(Name = "创建时间")]
	public DateTime? CreateTime { get; set; }
}
