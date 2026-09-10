using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 用户对象导入VO
/// </summary>
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
// Java 原注解 @Accessors(chain = true)（已注释掉，导入不允许使用 会找不到set方法）：C# 端不使用链式 setter
public class SysUserImportVo
{
	/// <summary>
	/// 用户ID
	/// </summary>
	[ExcelColumn(Name = "用户序号")]
	public long? UserId { get; set; }

	/// <summary>
	/// 部门ID
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "部门名称", converter = DeptExcelConverter.class)：MiniExcel 无转换器管线，
	// 部门名称需在导入后按名称自行匹配回 DeptId
	// Java 原注解 @ExcelDynamicOptions(providerClass = DeptExcelOptions.class)：C# 端无动态下拉选项实现
	[ExcelColumn(Name = "部门名称")]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	[ExcelColumn(Name = "用户账号")]
	public string UserName { get; set; }

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
	// 导入时需自行读取 [ExcelDictFormat] 的 DictType 反查取值
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
}
