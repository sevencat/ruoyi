using MiniExcelLibs.Attributes;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 字典类型视图对象 sys_dict_type
/// </summary>
// Java 原注解 @AutoMapper(target = SysDictType.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysDictTypeVo
{
	/// <summary>
	/// 字典主键
	/// </summary>
	[ExcelColumn(Name = "字典主键")]
	public long? DictId { get; set; }

	/// <summary>
	/// 字典名称
	/// </summary>
	[ExcelColumn(Name = "字典名称")]
	public string DictName { get; set; }

	/// <summary>
	/// 字典类型
	/// </summary>
	[ExcelColumn(Name = "字典类型")]
	public string DictType { get; set; }

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
}
