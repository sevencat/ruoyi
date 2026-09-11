using System.Text.Json.Serialization;
using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 部门视图对象 sys_dept
/// </summary>
// Java 原注解 @AutoMapper(target = SysDept.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysDeptVo
{
	/// <summary>
	/// 部门id
	/// </summary>
	[ExcelColumn(Name = "部门id")]
	[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
	public long? DeptId { get; set; }

	/// <summary>
	/// 父部门id
	/// </summary>
	[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
	public long? ParentId { get; set; }

	/// <summary>
	/// 父部门名称
	/// </summary>
	public string ParentName { get; set; }

	/// <summary>
	/// 祖级列表
	/// </summary>
	public string Ancestors { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	[ExcelColumn(Name = "部门名称")]
	public string DeptName { get; set; }

	/// <summary>
	/// 部门类别编码
	/// </summary>
	[ExcelColumn(Name = "部门类别编码")]
	public string DeptCategory { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	public int? OrderNum { get; set; }

	/// <summary>
	/// 负责人ID
	/// </summary>
	public long? Leader { get; set; }

	/// <summary>
	/// 负责人
	/// </summary>
	[ExcelColumn(Name = "负责人")]
	public string LeaderName { get; set; }

	/// <summary>
	/// 联系电话
	/// </summary>
	[ExcelColumn(Name = "联系电话")]
	public string Phone { get; set; }

	/// <summary>
	/// 邮箱
	/// </summary>
	[ExcelColumn(Name = "邮箱")]
	public string Email { get; set; }

	/// <summary>
	/// 部门状态（0正常 1停用）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "部门状态", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "部门状态")]
	[ExcelDictFormat(DictType = "sys_normal_disable")]
	public string Status { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	[ExcelColumn(Name = "创建时间")]
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 子部门
	/// </summary>
	public List<SysDeptVo> Children { get; set; } = [];
}
