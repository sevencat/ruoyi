namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 用户密码修改（对应 Java <c>SysProfileController</c> 内部的 record <c>SysUserPasswordBo</c>）
/// </summary>
// Java 校验注解（@NotBlank）：C# 端无等价校验管线，如需校验请自行使用 DataAnnotations/FluentValidation
public class SysUserPasswordBo
{
	/// <summary>
	/// 旧密码
	/// </summary>
	// Java 原注解 @NotBlank(message = "旧密码不能为空")
	public string OldPassword { get; set; }

	/// <summary>
	/// 新密码
	/// </summary>
	// Java 原注解 @NotBlank(message = "新密码不能为空")
	public string NewPassword { get; set; }
}
