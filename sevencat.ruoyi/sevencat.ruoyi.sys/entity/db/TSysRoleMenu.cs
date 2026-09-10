using FreeSql.DataAnnotations;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 角色和菜单关联表
/// </summary>
[Table(Name = "sys_role_menu")]
[Index("idx_sys_role_menu_mid", "MenuId")]
public class TSysRoleMenu
{
	/// <summary>
	/// 角色ID
	/// </summary>
	[Column(Name = "role_id", IsPrimary = true)]
	public long RoleId { get; set; }

	/// <summary>
	/// 菜单ID
	/// </summary>
	[Column(Name = "menu_id", IsPrimary = true)]
	public long MenuId { get; set; }
}