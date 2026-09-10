namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 登录用户信息
/// </summary>
public class UserInfoVo
{
	/// <summary>
	/// 用户基本信息
	/// </summary>
	public SysUserVo User { get; set; }

	/// <summary>
	/// 菜单权限
	/// </summary>
	public HashSet<string> Permissions { get; set; }

	/// <summary>
	/// 角色权限
	/// </summary>
	public HashSet<string> Roles { get; set; }
}
