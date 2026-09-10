using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 用户信息表
/// </summary>
[Table(Name = "sys_user")]
[Index("idx_sys_user_dept_id", "DeptId")]
[Index("idx_sys_user_create_by", "CreateBy")]
[Index("idx_sys_user_user_name", "UserName")]
[Index("idx_sys_user_phone", "PhoneNumber")]
public class TSysUser : TBaseEntity
{
	/// <summary>
	/// 用户ID
	/// </summary>
	[Column(Name = "user_id", IsPrimary = true)]
	[Snowflake]
	public long UserId { get; set; }

	/// <summary>
	/// 部门ID
	/// </summary>
	[Column(Name = "dept_id", IsNullable = true)]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户账号
	/// </summary>
	[Column(Name = "user_name", StringLength = 30, IsNullable = false)]
	public string UserName { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	[Column(Name = "nick_name", StringLength = 30, IsNullable = false)]
	public string NickName { get; set; }

	/// <summary>
	/// 用户类型（sys_user系统用户）
	/// </summary>
	[Column(Name = "user_type", StringLength = 10, IsNullable = true)]
	public string UserType { get; set; } = "sys_user";

	/// <summary>
	/// 用户邮箱
	/// </summary>
	[Column(Name = "email", StringLength = 50, IsNullable = true)]
	public string Email { get; set; } = string.Empty;

	/// <summary>
	/// 手机号码
	/// </summary>
	[Column(Name = "phone_number", StringLength = 11, IsNullable = true)]
	public string PhoneNumber { get; set; } = string.Empty;

	/// <summary>
	/// 用户性别（0男 1女 2未知）
	/// </summary>
	[Column(Name = "gender", StringLength = 1, IsNullable = true)]
	public string Gender { get; set; } = "0";

	/// <summary>
	/// 头像地址
	/// </summary>
	[Column(Name = "avatar", IsNullable = true)]
	public long? Avatar { get; set; }

	/// <summary>
	/// 密码
	/// </summary>
	[Column(Name = "password", StringLength = 100, IsNullable = true)]
	public string Password { get; set; } = string.Empty;

	/// <summary>
	/// 账号状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; } = "0";

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", StringLength = 1, IsNullable = true)]
	public string DelFlag { get; set; } = "0";

	/// <summary>
	/// 最后登录IP
	/// </summary>
	[Column(Name = "login_ip", StringLength = 128, IsNullable = true)]
	public string LoginIp { get; set; } = string.Empty;

	/// <summary>
	/// 最后登录时间
	/// </summary>
	[Column(Name = "login_date", IsNullable = true)]
	public DateTime? LoginDate { get; set; }
}