namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 社会化关系视图对象 sys_social
/// </summary>
// Java 原注解 @AutoMapper(target = SysSocial.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysSocialVo
{
	/// <summary>
	/// 主键
	/// </summary>
	public long? Id { get; set; }

	/// <summary>
	/// 用户ID
	/// </summary>
	public long? UserId { get; set; }

	/// <summary>
	/// 的唯一ID
	/// </summary>
	public string AuthId { get; set; }

	/// <summary>
	/// 用户来源
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// 用户的授权令牌
	/// </summary>
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
	/// 用户的 open id
	/// </summary>
	public string OpenId { get; set; }

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

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }
}
