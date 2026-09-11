using sevencat.ruoyi.common.constant;

namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 用户信息业务对象 sys_user
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
// Java 原注解 @AutoMapper(target = SysUser.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@Xss/@NotBlank/@Size/@Email 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysUserBo
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
	// Java 原注解 @Xss(message = "用户账号不能包含脚本字符")：C# 端无 XSS 校验注解，需自行过滤脚本字符
	// Java 原注解 @NotBlank(message = "用户账号不能为空")
	// Java 原注解 @Size(min = 2, max = 30, message = "用户账号长度必须在{min}到{max}个字符之间")
	public string UserName { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	// Java 原注解 @Xss(message = "用户昵称不能包含脚本字符")
	// Java 原注解 @NotBlank(message = "用户昵称不能为空")
	// Java 原注解 @Size(min = 0, max = 30, message = "用户昵称长度不能超过{max}个字符")
	public string NickName { get; set; }

	/// <summary>
	/// 用户类型（sys_user系统用户）
	/// </summary>
	public string UserType { get; set; }

	/// <summary>
	/// 用户邮箱
	/// </summary>
	// Java 原注解 @Email(message = "邮箱格式不正确")
	// Java 原注解 @Size(min = 0, max = 50, message = "邮箱长度不能超过{max}个字符")
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
	/// 密码
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	/// 账号状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 角色组
	/// </summary>
	// Java 原注解 @Size(min = 1, message = "用户角色不能为空")
	public long[] RoleIds { get; set; }

	/// <summary>
	/// 岗位组
	/// </summary>
	public long[] PostIds { get; set; }

	/// <summary>
	/// 数据权限 当前角色ID
	/// </summary>
	public long? RoleId { get; set; }

	/// <summary>
	/// 用户ID
	/// </summary>
	public string UserIds { get; set; }

	/// <summary>
	/// 排除不查询的用户(工作流用)
	/// </summary>
	public string ExcludeUserIds { get; set; }

	/// <summary>
	/// 创建者
	/// </summary>
	public long? CreateBy { get; set; }

	/// <summary>
	/// 更新者
	/// </summary>
	public long? UpdateBy { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();

	/// <summary>
	/// 判断当前用户是否为超级管理员。
	/// </summary>
	/// <returns>true 是超级管理员 false 不是超级管理员</returns>
	public bool IsSuperAdmin()
	{
		return UserId == SystemConstants.SUPER_ADMIN_USER_ID;
	}
}
