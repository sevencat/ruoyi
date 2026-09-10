using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 授权管理视图对象 sys_client
/// </summary>
// Java 原注解 @AutoMapper(target = SysClient.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysClientVo
{
	/// <summary>
	/// id
	/// </summary>
	[ExcelColumn(Name = "id")]
	public long? Id { get; set; }

	/// <summary>
	/// 客户端id
	/// </summary>
	[ExcelColumn(Name = "客户端id")]
	public string ClientId { get; set; }

	/// <summary>
	/// 客户端key
	/// </summary>
	[ExcelColumn(Name = "客户端key")]
	public string ClientKey { get; set; }

	/// <summary>
	/// 客户端秘钥
	/// </summary>
	[ExcelColumn(Name = "客户端秘钥")]
	public string ClientSecret { get; set; }

	/// <summary>
	/// 授权类型
	/// </summary>
	public List<string> GrantTypeList { get; set; }

	/// <summary>
	/// 授权类型
	/// </summary>
	[ExcelColumn(Name = "授权类型")]
	public string GrantType { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	public string DeviceType { get; set; }

	/// <summary>
	/// 允许访问路径
	/// </summary>
	[ExcelColumn(Name = "允许访问路径")]
	public string AccessPath { get; set; }

	/// <summary>
	/// 允许访问路径列表
	/// </summary>
	public List<string> AccessPathList { get; set; }

	/// <summary>
	/// IP白名单
	/// </summary>
	[ExcelColumn(Name = "IP白名单")]
	public string IpWhitelist { get; set; }

	/// <summary>
	/// IP白名单列表
	/// </summary>
	public List<string> IpWhitelistList { get; set; }

	/// <summary>
	/// token活跃超时时间
	/// </summary>
	[ExcelColumn(Name = "token活跃超时时间")]
	public long? ActiveTimeout { get; set; }

	/// <summary>
	/// token固定超时时间
	/// </summary>
	[ExcelColumn(Name = "token固定超时时间")]
	public long? Timeout { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "状态", converter = ExcelDictConvert.class) + @ExcelDictFormat(readConverterExp = "0=正常,1=停用")
	// MiniExcel 无字典转换注解，导出时需自行把状态映射为「正常/停用」（例如另建一个带展示字段的导出模型）
	[ExcelColumn(Name = "状态")]
	[ExcelConvert(typeof(ExcelDictConvert))]
	[ExcelDictFormat(ReadConverterExp = "0=正常,1=停用")]
	public string Status { get; set; }
}