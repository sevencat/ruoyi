using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 用户对象导出VO
/// </summary>
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
public class SysUserExportVo
{
	/// <summary>
	/// 用户ID
	/// </summary>
	[ExcelColumn(Name = "用户序号")]
	public long? UserId { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	[ExcelColumn(Name = "用户账号")]
	public string UserName { get; set; }

	/// <summary>
	/// 部门ID
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "部门名称", converter = DeptExcelConverter.class)：MiniExcel 无转换器管线，
	// 部门名称由 DeptId 转换后写入 DeptName 列，本属性只用于承载原始部门ID、不参与导出
	[ExcelIgnore]
	public long? DeptId { get; set; }

	/// <summary>
	/// 部门名称（Excel 列「部门名称」，导出前按 DeptId 转换为「父级/子级」全路径名称）
	/// </summary>
	[ExcelColumn(Name = "部门名称")]
	public string DeptName { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	[ExcelColumn(Name = "用户昵称")]
	public string NickName { get; set; }

	/// <summary>
	/// 用户邮箱
	/// </summary>
	[ExcelColumn(Name = "用户邮箱")]
	public string Email { get; set; }

	/// <summary>
	/// 手机号码
	/// </summary>
	[ExcelColumn(Name = "手机号码")]
	public string PhoneNumber { get; set; }

	/// <summary>
	/// 用户性别（0男 1女 2未知）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "用户性别", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 的 DictType 转换取值
	[ExcelColumn(Name = "用户性别")]
	[ExcelDictFormat(DictType = "sys_user_gender")]
	public string Gender { get; set; }

	/// <summary>
	/// 账号状态（0正常 1停用）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "账号状态", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线
	[ExcelColumn(Name = "账号状态")]
	[ExcelDictFormat(DictType = "sys_normal_disable")]
	public string Status { get; set; }

	/// <summary>
	/// 最后登录IP
	/// </summary>
	[ExcelColumn(Name = "最后登录IP")]
	public string LoginIp { get; set; }

	/// <summary>
	/// 最后登录时间
	/// </summary>
	[ExcelColumn(Name = "最后登录时间")]
	public DateTime? LoginDate { get; set; }

	/// <summary>
	/// 负责人
	/// </summary>
	[ExcelColumn(Name = "部门负责人")]
	public string LeaderName { get; set; }
}
