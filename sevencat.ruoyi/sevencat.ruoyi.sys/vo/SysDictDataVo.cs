using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 字典数据视图对象 sys_dict_data
/// </summary>
// Java 原注解 @AutoMapper(target = SysDictData.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysDictDataVo
{
	/// <summary>
	/// 字典编码
	/// </summary>
	[ExcelColumn(Name = "字典编码")]
	public long? DictCode { get; set; }

	/// <summary>
	/// 字典排序
	/// </summary>
	[ExcelColumn(Name = "字典排序")]
	public int? DictSort { get; set; }

	/// <summary>
	/// 字典标签
	/// </summary>
	[ExcelColumn(Name = "字典标签")]
	public string DictLabel { get; set; }

	/// <summary>
	/// 字典键值
	/// </summary>
	[ExcelColumn(Name = "字典键值")]
	public string DictValue { get; set; }

	/// <summary>
	/// 字典类型
	/// </summary>
	[ExcelColumn(Name = "字典类型")]
	public string DictType { get; set; }

	/// <summary>
	/// 样式属性（其他样式扩展）
	/// </summary>
	public string CssClass { get; set; }

	/// <summary>
	/// 表格回显样式
	/// </summary>
	public string ListClass { get; set; }

	/// <summary>
	/// 是否默认（Y是 N否）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "是否默认", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "是否默认")]
	[ExcelDictFormat(DictType = "sys_yes_no")]
	public string IsDefault { get; set; }

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
