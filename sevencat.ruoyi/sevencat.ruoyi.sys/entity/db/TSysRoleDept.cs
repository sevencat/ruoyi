using FreeSql.DataAnnotations;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 角色和部门关联表（自定义数据权限）
/// </summary>
[Table(Name = "sys_role_dept")]
[Index("idx_sys_role_dept_did", "DeptId")]
public class TSysRoleDept
{
	/// <summary>
	/// 角色ID
	/// </summary>
	[Column(Name = "role_id", IsPrimary = true)]
	public long RoleId { get; set; }

	/// <summary>
	/// 部门ID
	/// </summary>
	[Column(Name = "dept_id", IsPrimary = true)]
	public long DeptId { get; set; }
}
