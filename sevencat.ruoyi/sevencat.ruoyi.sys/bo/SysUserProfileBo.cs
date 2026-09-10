namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 个人信息业务处理
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
// Java 校验注解（@Xss/@Size/@Email/@Pattern 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysUserProfileBo
{
	/// <summary>
	/// 用户昵称
	/// </summary>
	// Java 原注解 @Xss(message = "用户昵称不能包含脚本字符")：C# 端无 XSS 校验注解，需自行过滤脚本字符
	// Java 原注解 @Size(min = 0, max = 30, message = "用户昵称长度不能超过{max}个字符")
	public string NickName { get; set; }

	/// <summary>
	/// 用户邮箱
	/// </summary>
	// Java 原注解 @Sensitive(strategy = SensitiveStrategy.EMAIL)：C# 端无脱敏注解，需在序列化/输出层自行做脱敏处理
	// Java 原注解 @Email(message = "邮箱格式不正确")
	// Java 原注解 @Size(min = 0, max = 50, message = "邮箱长度不能超过{max}个字符")
	public string Email { get; set; }

	/// <summary>
	/// 手机号码
	/// </summary>
	// Java 原注解 @Pattern(regexp = RegexConstants.MOBILE, message = "手机号格式不正确")
	// Java 原注解 @Sensitive(strategy = SensitiveStrategy.PHONE)：C# 端无脱敏注解
	public string PhoneNumber { get; set; }

	/// <summary>
	/// 用户性别（0男 1女 2未知）
	/// </summary>
	public string Gender { get; set; }

	/// <summary>
	/// 头像 OSS ID
	/// </summary>
	public long? Avatar { get; set; }
}
