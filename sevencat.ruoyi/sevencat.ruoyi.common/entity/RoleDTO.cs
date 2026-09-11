namespace sevencat.ruoyi.common.entity;

/// <summary>
/// 角色简要信息对象
/// </summary>
public class RoleDTO
{
	/// <summary>
	/// 角色ID
	/// </summary>
	public long? RoleId { get; set; }

	/// <summary>
	/// 角色名称
	/// </summary>
	public string RoleName { get; set; }

	/// <summary>
	/// 角色权限
	/// </summary>
	public string RoleKey { get; set; }

	/// <summary>
	/// 数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5：仅本人数据权限 6：部门及以下或本人数据权限）
	/// </summary>
	public string DataScope { get; set; }
}
