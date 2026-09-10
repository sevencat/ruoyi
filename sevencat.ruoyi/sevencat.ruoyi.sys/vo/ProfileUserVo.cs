namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 用户信息视图对象 sys_user
/// </summary>
public class ProfileUserVo
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
	/// 头像 OSS ID
	/// </summary>
	public long? Avatar { get; set; }

	/// <summary>
	/// 头像地址
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.OSS_ID_TO_URL, mapper = "avatar")：C# 端无翻译注解，
	// 需在查询后自行按 Avatar 回填地址
	public string AvatarUrl { get; set; }

	/// <summary>
	/// 最后登录IP
	/// </summary>
	public string LoginIp { get; set; }

	/// <summary>
	/// 最后登录时间
	/// </summary>
	public DateTime? LoginDate { get; set; }

	/// <summary>
	/// 部门名
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.DEPT_ID_TO_NAME, mapper = "deptId")：C# 端无翻译注解，
	// 需在查询后自行按 DeptId 回填名称
	public string DeptName { get; set; }
}
