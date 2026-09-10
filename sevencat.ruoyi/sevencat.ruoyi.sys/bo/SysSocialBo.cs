namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 社会化关系业务对象 sys_social
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
// Java 原注解 @AutoMapper(target = SysSocial.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@NotNull 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysSocialBo
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 原注解 @NotNull(message = "主键不能为空", groups = {EditGroup.class})
	public long? Id { get; set; }

	/// <summary>
	/// 认证唯一ID
	/// </summary>
	// Java 原注解 @NotBlank(message = "认证唯一ID不能为空", groups = {AddGroup.class, EditGroup.class})
	public string AuthId { get; set; }

	/// <summary>
	/// 用户来源
	/// </summary>
	// Java 原注解 @NotBlank(message = "用户来源不能为空", groups = {AddGroup.class, EditGroup.class})
	public string Source { get; set; }

	/// <summary>
	/// 用户的授权令牌
	/// </summary>
	// Java 原注解 @NotBlank(message = "用户的授权令牌不能为空", groups = {AddGroup.class, EditGroup.class})
	public string AccessToken { get; set; }

	/// <summary>
	/// 用户的授权令牌的有效期，部分平台可能没有
	/// </summary>
	public int ExpireIn { get; set; }

	/// <summary>
	/// 刷新令牌，部分平台可能没有
	/// </summary>
	public string RefreshToken { get; set; }

	/// <summary>
	/// 平台唯一id
	/// </summary>
	public string OpenId { get; set; }

	/// <summary>
	/// 用户的 ID
	/// </summary>
	// Java 原注解 @NotBlank(message = "用户的ID不能为空", groups = {AddGroup.class, EditGroup.class})
	public long? UserId { get; set; }

	/// <summary>
	/// 平台的授权信息，部分平台可能没有
	/// </summary>
	public string AccessCode { get; set; }

	/// <summary>
	/// 用户的 unionid
	/// </summary>
	public string UnionId { get; set; }

	/// <summary>
	/// 授予的权限，部分平台可能没有
	/// </summary>
	public string Scope { get; set; }

	/// <summary>
	/// 授权的第三方账号
	/// </summary>
	public string UserName { get; set; }

	/// <summary>
	/// 授权的第三方昵称
	/// </summary>
	public string NickName { get; set; }

	/// <summary>
	/// 授权的第三方邮箱
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// 授权的第三方头像地址
	/// </summary>
	public string Avatar { get; set; }

	/// <summary>
	/// 个别平台的授权信息，部分平台可能没有
	/// </summary>
	public string TokenType { get; set; }

	/// <summary>
	/// id token，部分平台可能没有
	/// </summary>
	public string IdToken { get; set; }

	/// <summary>
	/// 小米平台用户的附带属性，部分平台可能没有
	/// </summary>
	public string MacAlgorithm { get; set; }

	/// <summary>
	/// 小米平台用户的附带属性，部分平台可能没有
	/// </summary>
	public string MacKey { get; set; }

	/// <summary>
	/// 用户的授权code，部分平台可能没有
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	/// Twitter平台用户的附带属性，部分平台可能没有
	/// </summary>
	public string OauthToken { get; set; }

	/// <summary>
	/// Twitter平台用户的附带属性，部分平台可能没有
	/// </summary>
	public string OauthTokenSecret { get; set; }
}
