using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.dto;

namespace sevencat.ruoyi.sys.entity;

/// <summary>
/// 登录用户身份信息（缓存于登录态中）
/// </summary>
public class LoginUser
{
	/// <summary>
	/// 用户ID
	/// </summary>
	public long? UserId { get; set; }

	/// <summary>
	/// 部门ID
	/// </summary>
	public long? DeptId { get; set; }

	/// <summary>
	/// 部门类别编码
	/// </summary>
	public string DeptCategory { get; set; }

	/// <summary>
	/// 部门名
	/// </summary>
	public string DeptName { get; set; }

	/// <summary>
	/// 用户唯一标识
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	/// 用户类型
	/// </summary>
	public string UserType { get; set; }

	/// <summary>
	/// 登录时间（Unix 毫秒时间戳）
	/// </summary>
	public long? LoginTime { get; set; }

	/// <summary>
	/// 过期时间（Unix 毫秒时间戳）
	/// </summary>
	public long? ExpireTime { get; set; }

	/// <summary>
	/// 登录IP地址
	/// </summary>
	public string Ipaddr { get; set; }

	/// <summary>
	/// 登录地点
	/// </summary>
	public string LoginLocation { get; set; }

	/// <summary>
	/// 浏览器类型
	/// </summary>
	public string Browser { get; set; }

	/// <summary>
	/// 操作系统
	/// </summary>
	public string Os { get; set; }

	/// <summary>
	/// 菜单权限
	/// </summary>
	public HashSet<string> MenuPermission { get; set; } = [];

	/// <summary>
	/// 角色权限
	/// </summary>
	public HashSet<string> RolePermission { get; set; } = [];

	/// <summary>
	/// 用户名
	/// </summary>
	public string Username { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	public string Nickname { get; set; }

	/// <summary>
	/// 角色对象
	/// </summary>
	public List<RoleDTO> Roles { get; set; } = [];

	/// <summary>
	/// 数据权限角色映射：key 为权限码，value 为可参与数据权限计算的角色ID列表
	/// </summary>
	public Dictionary<string, List<long>> DataScopeRoleMap { get; set; } = [];

	/// <summary>
	/// 岗位对象
	/// </summary>
	public List<PostDTO> Posts { get; set; } = [];

	/// <summary>
	/// 数据权限：当前角色ID
	/// </summary>
	public long? RoleId { get; set; }

	/// <summary>
	/// 客户端
	/// </summary>
	public string ClientKey { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	public string DeviceType { get; set; }

	/// <summary>
	/// 获取 Sa-Token 使用的登录标识
	/// </summary>
	/// <exception cref="ArgumentException">用户类型或用户ID为空时抛出</exception>
	public string GetLoginId()
	{
		if (UserType is null)
		{
			throw new ArgumentException("用户类型不能为空");
		}

		if (UserId is null)
		{
			throw new ArgumentException("用户ID不能为空");
		}

		return $"{UserType}:{UserId}";
	}

	public bool IsSuperAdmin()
	{
		return SystemConstants.SUPER_ADMIN_USER_ID == UserId;
	}

	public static bool IsSuperAdmin(long UserId)
	{
		return SystemConstants.SUPER_ADMIN_USER_ID == UserId;
	}
}