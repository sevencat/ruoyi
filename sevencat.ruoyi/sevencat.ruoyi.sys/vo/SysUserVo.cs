using System.Text.Json.Serialization;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 用户信息视图对象 sys_user
/// </summary>
// Java 原注解 @AutoMapper(target = SysUser.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysUserVo
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
	// Java 原注解 @Sensitive(strategy = SensitiveStrategy.EMAIL, perms = "system:user:edit")：C# 端无脱敏注解，
	// 需在序列化/输出层自行做脱敏处理
	public string Email { get; set; }

	/// <summary>
	/// 手机号码
	/// </summary>
	// Java 原注解 @Sensitive(strategy = SensitiveStrategy.PHONE, perms = "system:user:edit")：C# 端无脱敏注解
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
	/// 密码
	/// </summary>
	// Java 原注解 @JsonIgnore + @JsonProperty：允许反序列化（接收前端传入），但不参与序列化（不返回给前端）
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
	public string Password { get; set; }

	/// <summary>
	/// 账号状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 最后登录IP
	/// </summary>
	public string LoginIp { get; set; }

	/// <summary>
	/// 最后登录时间
	/// </summary>
	public DateTime? LoginDate { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 更新时间
	/// </summary>
	public DateTime? UpdateTime { get; set; }

	/// <summary>
	/// 部门名
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.DEPT_ID_TO_NAME, mapper = "deptId")：C# 端无翻译注解，
	// 需在查询后自行按 DeptId 回填名称
	public string DeptName { get; set; }

	/// <summary>
	/// 角色对象
	/// </summary>
	public List<SysRoleVo> Roles { get; set; }

	/// <summary>
	/// 角色组
	/// </summary>
	public long[] RoleIds { get; set; }

	/// <summary>
	/// 岗位组
	/// </summary>
	public long[] PostIds { get; set; }

	/// <summary>
	/// 数据权限 当前角色ID
	/// </summary>
	public long? RoleId { get; set; }
}
