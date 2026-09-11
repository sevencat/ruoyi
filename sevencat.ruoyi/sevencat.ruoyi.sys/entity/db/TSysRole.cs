using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 角色信息表
/// </summary>
[Table(Name = "sys_role")]
[Index("idx_sys_role_create_dept", "CreateDept")]
[Index("idx_sys_role_create_by", "CreateBy")]
[DataScope(DeptColumn = nameof(TBaseEntity.CreateDept))]
public class TSysRole : TBaseEntity
{
	/// <summary>
	/// 角色ID
	/// </summary>
	[Column(Name = "role_id", IsPrimary = true)]
	[Snowflake]
	public long RoleId { get; set; }

	/// <summary>
	/// 角色名称
	/// </summary>
	[Column(Name = "role_name", StringLength = 30, IsNullable = false)]
	public string RoleName { get; set; }

	/// <summary>
	/// 角色权限字符串
	/// </summary>
	[Column(Name = "role_key", StringLength = 100, IsNullable = false)]
	public string RoleKey { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	[Column(Name = "role_sort", IsNullable = false)]
	public int RoleSort { get; set; }

	/// <summary>
	/// 数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5：仅本人数据权限 6：部门及以下或本人数据权限）
	/// </summary>
	[Column(Name = "data_scope", StringLength = 1, IsNullable = true)]
	public string DataScope { get; set; } = "1";

	/// <summary>
	/// 菜单树选择项是否关联显示
	/// </summary>
	[Column(Name = "menu_check_strictly", IsNullable = true)]
	public bool? MenuCheckStrictly { get; set; } = true;

	/// <summary>
	/// 部门树选择项是否关联显示
	/// </summary>
	[Column(Name = "dept_check_strictly", IsNullable = true)]
	public bool? DeptCheckStrictly { get; set; } = true;

	/// <summary>
	/// 角色状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = false)]
	public string Status { get; set; }

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", StringLength = 1, IsNullable = true)]
	public string DelFlag { get; set; } = "0";
}
