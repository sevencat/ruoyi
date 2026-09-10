using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 岗位信息视图对象 sys_post
/// </summary>
// Java 原注解 @AutoMapper(target = SysPost.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysPostVo
{
	/// <summary>
	/// 岗位ID
	/// </summary>
	[ExcelColumn(Name = "岗位序号")]
	public long? PostId { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	[ExcelColumn(Name = "部门id")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 岗位编码
	/// </summary>
	[ExcelColumn(Name = "岗位编码")]
	public string PostCode { get; set; }

	/// <summary>
	/// 岗位名称
	/// </summary>
	[ExcelColumn(Name = "岗位名称")]
	public string PostName { get; set; }

	/// <summary>
	/// 岗位类别编码
	/// </summary>
	[ExcelColumn(Name = "类别编码")]
	public string PostCategory { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	[ExcelColumn(Name = "岗位排序")]
	public int? PostSort { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "状态", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "状态")]
	[ExcelDictFormat(DictType = "sys_normal_disable")]
	public string Status { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	[ExcelColumn(Name = "备注")]
	public string Remark { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	[ExcelColumn(Name = "创建时间")]
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 部门名
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.DEPT_ID_TO_NAME, mapper = "deptId")：C# 端无翻译注解，
	// 需在查询后自行按 DeptId 回填名称
	public string DeptName { get; set; }
}
