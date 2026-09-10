namespace sevencat.ruoyi.sys.entity.dto;

/// <summary>
/// 用户
/// </summary>
public class UserDTO
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
	/// 用户账号
	/// </summary>
	public string UserName { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	public string NickName { get; set; }

	/// <summary>
	/// 用户类型（sys_user系统用户）
	/// </summary>
	public string UserType { get; set; }

	/// <summary>
	/// 用户邮箱
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// 手机号码
	/// </summary>
	public string PhoneNumber { get; set; }

	/// <summary>
	/// 用户性别（0男 1女 2未知）
	/// </summary>
	public string Gender { get; set; }

	/// <summary>
	/// 账号状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }
}
