using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 参数配置视图对象 sys_config
/// </summary>
// Java 原注解 @AutoMapper(target = SysConfig.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysConfigVo
{
	/// <summary>
	/// 参数主键
	/// </summary>
	[ExcelColumn(Name = "参数主键")]
	public long? ConfigId { get; set; }

	/// <summary>
	/// 参数名称
	/// </summary>
	[ExcelColumn(Name = "参数名称")]
	public string ConfigName { get; set; }

	/// <summary>
	/// 参数键名
	/// </summary>
	[ExcelColumn(Name = "参数键名")]
	public string ConfigKey { get; set; }

	/// <summary>
	/// 参数键值
	/// </summary>
	[ExcelColumn(Name = "参数键值")]
	public string ConfigValue { get; set; }

	/// <summary>
	/// 系统内置（Y是 N否）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "系统内置", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "系统内置")]
	[ExcelDictFormat(DictType = "sys_yes_no")]
	public string ConfigType { get; set; }

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
