using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 社会化关系表 sys_social
/// </summary>
/// <remarks>
/// Java 实体 <c>SysSocial</c> 未声明 <c>del_flag</c> / <c>@TableLogic</c>，
/// 因此查询不会隐式追加 <c>del_flag = '0'</c>，删除也是物理删除（与本项目其它带逻辑删除的表不同），
/// 该字段仅由数据库默认值维护，C# 侧保持一致不做映射。
/// </remarks>
[Table(Name = "sys_social")]
public class TSysSocial : TBaseEntity
{
	/// <summary>
	/// 主键
	/// </summary>
	[Column(Name = "id", IsPrimary = true)]
	[Snowflake]
	public long Id { get; set; }

	/// <summary>
	/// 用户ID
	/// </summary>
	[Column(Name = "user_id")]
	public long UserId { get; set; }

	/// <summary>
	/// 平台+平台唯一id
	/// </summary>
	[Column(Name = "auth_id", StringLength = 255, IsNullable = true)]
	public string AuthId { get; set; }

	/// <summary>
	/// 用户来源
	/// </summary>
	[Column(Name = "source", StringLength = 255, IsNullable = true)]
	public string Source { get; set; }

	/// <summary>
	/// 平台编号唯一id
	/// </summary>
	[Column(Name = "open_id", StringLength = 255, IsNullable = true)]
	public string OpenId { get; set; }

	/// <summary>
	/// 登录账号
	/// </summary>
	[Column(Name = "user_name", StringLength = 30, IsNullable = true)]
	public string UserName { get; set; }

	/// <summary>
	/// 用户昵称
	/// </summary>
	[Column(Name = "nick_name", StringLength = 30, IsNullable = true)]
	public string NickName { get; set; }

	/// <summary>
	/// 用户邮箱
	/// </summary>
	[Column(Name = "email", StringLength = 255, IsNullable = true)]
	public string Email { get; set; }

	/// <summary>
	/// 头像地址
	/// </summary>
	[Column(Name = "avatar", StringLength = 500, IsNullable = true)]
	public string Avatar { get; set; }

	/// <summary>
	/// 用户的授权令牌
	/// </summary>
	[Column(Name = "access_token", StringLength = 2000, IsNullable = true)]
	public string AccessToken { get; set; }

	/// <summary>
	/// 用户的授权令牌的有效期，部分平台可能没有
	/// </summary>
	[Column(Name = "expire_in", IsNullable = true)]
	public int? ExpireIn { get; set; }

	/// <summary>
	/// 刷新令牌，部分平台可能没有
	/// </summary>
	[Column(Name = "refresh_token", StringLength = 2000, IsNullable = true)]
	public string RefreshToken { get; set; }

	/// <summary>
	/// 平台的授权信息，部分平台可能没有
	/// </summary>
	[Column(Name = "access_code", StringLength = 255, IsNullable = true)]
	public string AccessCode { get; set; }

	/// <summary>
	/// 用户的 unionid
	/// </summary>
	[Column(Name = "union_id", StringLength = 255, IsNullable = true)]
	public string UnionId { get; set; }

	/// <summary>
	/// 授予的权限，部分平台可能没有
	/// </summary>
	[Column(Name = "scope", StringLength = 255, IsNullable = true)]
	public string Scope { get; set; }

	/// <summary>
	/// 个别平台的授权信息，部分平台可能没有
	/// </summary>
	[Column(Name = "token_type", StringLength = 255, IsNullable = true)]
	public string TokenType { get; set; }

	/// <summary>
	/// id token，部分平台可能没有
	/// </summary>
	[Column(Name = "id_token", StringLength = 2000, IsNullable = true)]
	public string IdToken { get; set; }

	/// <summary>
	/// 小米平台用户的附带属性，部分平台可能没有
	/// </summary>
	[Column(Name = "mac_algorithm", StringLength = 255, IsNullable = true)]
	public string MacAlgorithm { get; set; }

	/// <summary>
	/// 小米平台用户的附带属性，部分平台可能没有
	/// </summary>
	[Column(Name = "mac_key", StringLength = 255, IsNullable = true)]
	public string MacKey { get; set; }

	/// <summary>
	/// 用户的授权code，部分平台可能没有
	/// </summary>
	[Column(Name = "code", StringLength = 255, IsNullable = true)]
	public string Code { get; set; }

	/// <summary>
	/// Twitter平台用户的附带属性，部分平台可能没有
	/// </summary>
	[Column(Name = "oauth_token", StringLength = 255, IsNullable = true)]
	public string OauthToken { get; set; }

	/// <summary>
	/// Twitter平台用户的附带属性，部分平台可能没有
	/// </summary>
	[Column(Name = "oauth_token_secret", StringLength = 255, IsNullable = true)]
	public string OauthTokenSecret { get; set; }
}
