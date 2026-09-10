namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 用户信息
/// </summary>
public class SysUserInfoVo
{
	/// <summary>
	/// 用户信息
	/// </summary>
	public SysUserVo User { get; set; }

	/// <summary>
	/// 角色ID列表
	/// </summary>
	public List<long?> RoleIds { get; set; }

	/// <summary>
	/// 角色列表
	/// </summary>
	public List<SysRoleVo> Roles { get; set; }

	/// <summary>
	/// 岗位ID列表
	/// </summary>
	public List<long?> PostIds { get; set; }

	/// <summary>
	/// 岗位列表
	/// </summary>
	public List<SysPostVo> Posts { get; set; }
}
