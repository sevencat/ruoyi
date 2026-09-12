using MiniExcelLibs.Attributes;

namespace sevencat.ruoyi.demo.vo;

/// <summary>
/// 测试单表视图对象 test_demo
/// </summary>
/// Java 原注解 @AutoMapper(target = TestDemo.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class TestDemoVo
{
	/// <summary>
	/// 主键
	/// </summary>
	[ExcelColumn(Name = "主键")]
	public long? Id { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	// Java 原注解 @ExcelRequired：C# 端 MiniExcel 无「必填」标记，导入侧由 Service 手工校验
	[ExcelColumn(Name = "部门id")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户id
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "用户id", index = 5)：MiniExcel 按属性声明顺序输出列，不再使用 index
	[ExcelColumn(Name = "用户id")]
	public long? UserId { get; set; }

	/// <summary>
	/// 排序号
	/// </summary>
	[ExcelColumn(Name = "排序号")]
	public int? OrderNum { get; set; }

	/// <summary>
	/// key键
	/// </summary>
	// Java 原注解 @ExcelNotation(value = "测试key")：C# 端 MiniExcel 无批注支持，未实现
	[ExcelColumn(Name = "key键")]
	public string TestKey { get; set; }

	/// <summary>
	/// 值
	/// </summary>
	// Java 原注解 @ExcelNotation(value = "测试value")：C# 端 MiniExcel 无批注支持，未实现
	[ExcelColumn(Name = "值")]
	public string Value { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	// Java 原注解 @DateTimeFormat("yyyy-MM-dd HH:mm:ss")：MiniExcel 按属性类型默认格式输出
	[ExcelColumn(Name = "创建时间")]
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 创建人
	/// </summary>
	[ExcelColumn(Name = "创建人")]
	public long? CreateBy { get; set; }

	/// <summary>
	/// 创建人账号
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "createBy")：C# 端无翻译注解，
	// 需在查询后自行按 CreateBy 回填名称
	[ExcelColumn(Name = "创建人账号")]
	public string CreateByName { get; set; }

	/// <summary>
	/// 更新时间
	/// </summary>
	[ExcelColumn(Name = "更新时间")]
	public DateTime? UpdateTime { get; set; }

	/// <summary>
	/// 更新人
	/// </summary>
	[ExcelColumn(Name = "更新人")]
	public long? UpdateBy { get; set; }

	/// <summary>
	/// 更新人账号
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "updateBy")：C# 端无翻译注解，
	// 需在查询后自行按 UpdateBy 回填名称
	[ExcelColumn(Name = "更新人账号")]
	public string UpdateByName { get; set; }

	/// <summary>
	/// 版本
	/// </summary>
	// Java 端无 @ExcelProperty，被类级 @ExcelIgnoreUnannotated 排除，这里用 [ExcelIgnore] 等价表达
	[ExcelIgnore]
	public int? Version { get; set; }
}
