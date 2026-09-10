using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 系统访问记录视图对象 sys_login_info
/// </summary>
// Java 原注解 @AutoMapper(target = SysLoginInfo.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysLoginInfoVo
{
	/// <summary>
	/// 访问ID
	/// </summary>
	[ExcelColumn(Name = "序号")]
	public long? InfoId { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	[ExcelColumn(Name = "用户账号")]
	public string UserName { get; set; }

	/// <summary>
	/// 客户端
	/// </summary>
	[ExcelColumn(Name = "客户端")]
	public string ClientKey { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "设备类型", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "设备类型")]
	[ExcelDictFormat(DictType = "sys_device_type")]
	public string DeviceType { get; set; }

	/// <summary>
	/// 登录状态（0成功 1失败）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "登录状态", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线
	[ExcelColumn(Name = "登录状态")]
	[ExcelDictFormat(DictType = "sys_common_status")]
	public string Status { get; set; }

	/// <summary>
	/// 登录IP地址
	/// </summary>
	[ExcelColumn(Name = "登录地址")]
	public string Ipaddr { get; set; }

	/// <summary>
	/// 登录地点
	/// </summary>
	[ExcelColumn(Name = "登录地点")]
	public string LoginLocation { get; set; }

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
	/// 提示消息
	/// </summary>
	[ExcelColumn(Name = "提示消息")]
	public string Msg { get; set; }

	/// <summary>
	/// 访问时间
	/// </summary>
	[ExcelColumn(Name = "访问时间")]
	public DateTime? LoginTime { get; set; }
}
