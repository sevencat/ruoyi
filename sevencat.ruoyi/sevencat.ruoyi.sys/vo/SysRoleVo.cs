using MiniExcelLibs.Attributes;
using sevencat.ruoyi.common.excel.attr;
using sevencat.ruoyi.sys.constant;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 角色信息视图对象 sys_role
/// </summary>
// Java 原注解 @AutoMapper(target = SysRole.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysRoleVo
{
	/// <summary>
	/// 角色ID
	/// </summary>
	[ExcelColumn(Name = "角色序号")]
	public long? RoleId { get; set; }

	/// <summary>
	/// 角色名称
	/// </summary>
	[ExcelColumn(Name = "角色名称")]
	public string RoleName { get; set; }

	/// <summary>
	/// 角色权限字符串
	/// </summary>
	[ExcelColumn(Name = "角色权限")]
	public string RoleKey { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	[ExcelColumn(Name = "角色排序")]
	public int RoleSort { get; set; }

	/// <summary>
	/// 数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5：仅本人数据权限 6：部门及以下或本人数据权限）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "数据范围", converter = ExcelDictConvert.class)：MiniExcel 无转换器管线，
	// 导出时需自行读取 [ExcelDictFormat] 按 ReadConverterExp 转换
	[ExcelColumn(Name = "数据范围")]
	[ExcelDictFormat(ReadConverterExp = "1=全部数据权限,2=自定义数据权限,3=本部门数据权限,4=本部门及以下数据权限,5=仅本人数据权限,6=部门及以下或本人数据权限")]
	public string DataScope { get; set; }

	/// <summary>
	/// 菜单树选择项是否关联显示
	/// </summary>
	[ExcelColumn(Name = "菜单树选择项是否关联显示")]
	public bool? MenuCheckStrictly { get; set; }

	/// <summary>
	/// 部门树选择项是否关联显示
	/// </summary>
	[ExcelColumn(Name = "部门树选择项是否关联显示")]
	public bool? DeptCheckStrictly { get; set; }

	/// <summary>
	/// 角色状态（0正常 1停用）
	/// </summary>
	// Java 原注解 @ExcelProperty(value = "角色状态", converter = ExcelDictConvert.class)：同上，转换器部分无对应实现
	[ExcelColumn(Name = "角色状态")]
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
	/// 用户是否存在此角色标识 默认不存在
	/// </summary>
	public bool Flag { get; set; } = false;

	/// <summary>
	/// 判断当前角色是否为超级管理员角色
	/// </summary>
	/// <returns>true 是超级管理员角色 false 不是超级管理员角色</returns>
	public bool IsSuperAdmin()
	{
		return RoleId == SystemConstants.SUPER_ADMIN_ROLE_ID;
	}
}
