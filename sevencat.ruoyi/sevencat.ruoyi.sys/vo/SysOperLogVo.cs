using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 操作日志记录视图对象 sys_oper_log
/// </summary>
// Java 原注解 @AutoMapper(target = SysOperLog.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysOperLogVo
{
	/// <summary>
	/// 日志主键
	/// </summary>
	[ExcelColumn(Name = "日志主键")]
	public long? OperId { get; set; }

	/// <summary>
	/// 模块标题
	/// </summary>
	[ExcelColumn(Name = "操作模块")]
	public string Title { get; set; }

	/// <summary>
	/// 业务类型（0其它 1新增 2修改 3删除）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "业务类型", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "业务类型")]
	[ExcelDictFormat(DictType = "sys_oper_type")]
	public int? BusinessType { get; set; }

	/// <summary>
	/// 业务类型数组
	/// </summary>
	public int[] BusinessTypes { get; set; }

	/// <summary>
	/// 方法名称
	/// </summary>
	[ExcelColumn(Name = "请求方法")]
	public string Method { get; set; }

	/// <summary>
	/// 请求方式
	/// </summary>
	[ExcelColumn(Name = "请求方式")]
	public string RequestMethod { get; set; }

	/// <summary>
	/// 操作类别（0其它 1后台用户 2手机端用户）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "操作类别", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 ReadConverterExp 转换取值
	[ExcelColumn(Name = "操作类别")]
	[ExcelDictFormat(ReadConverterExp = "0=其它,1=后台用户,2=手机端用户")]
	public int? OperatorType { get; set; }

	/// <summary>
	/// 操作人员
	/// </summary>
	[ExcelColumn(Name = "操作人员")]
	public string OperName { get; set; }

	/// <summary>
	/// 操作用户ID
	/// </summary>
	[ExcelColumn(Name = "操作用户ID")]
	public long? UserId { get; set; }

	/// <summary>
	/// 操作部门ID
	/// </summary>
	[ExcelColumn(Name = "操作部门ID")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	[ExcelColumn(Name = "部门名称")]
	public string DeptName { get; set; }

	/// <summary>
	/// 客户端
	/// </summary>
	[ExcelColumn(Name = "客户端")]
	public string ClientKey { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "设备类型", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线
	[ExcelColumn(Name = "设备类型")]
	[ExcelDictFormat(DictType = "sys_device_type")]
	public string DeviceType { get; set; }

	/// <summary>
	/// 浏览器类型
	/// </summary>
	[ExcelColumn(Name = "浏览器")]
	public string Browser { get; set; }

	/// <summary>
	/// 操作系统
	/// </summary>
	[ExcelColumn(Name = "操作系统")]
	public string Os { get; set; }

	/// <summary>
	/// 请求URL
	/// </summary>
	[ExcelColumn(Name = "请求地址")]
	public string OperUrl { get; set; }

	/// <summary>
	/// 主机地址
	/// </summary>
	[ExcelColumn(Name = "操作地址")]
	public string OperIp { get; set; }

	/// <summary>
	/// 操作地点
	/// </summary>
	[ExcelColumn(Name = "操作地点")]
	public string OperLocation { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	[ExcelColumn(Name = "请求参数")]
	public string OperParam { get; set; }

	/// <summary>
	/// 返回参数
	/// </summary>
	[ExcelColumn(Name = "返回参数")]
	public string JsonResult { get; set; }

	/// <summary>
	/// 操作状态（0正常 1异常）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "状态", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线
	[ExcelColumn(Name = "状态")]
	[ExcelDictFormat(DictType = "sys_common_status")]
	public int? Status { get; set; }

	/// <summary>
	/// 错误消息
	/// </summary>
	[ExcelColumn(Name = "错误消息")]
	public string ErrorMsg { get; set; }

	/// <summary>
	/// 操作时间
	/// </summary>
	[ExcelColumn(Name = "操作时间")]
	public DateTime? OperTime { get; set; }

	/// <summary>
	/// 消耗时间
	/// </summary>
	[ExcelColumn(Name = "消耗时间")]
	public long? CostTime { get; set; }
}
