using FreeSql.DataAnnotations;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 用户和角色关联表
/// </summary>
[Table(Name = "sys_user_role")]
[Index("idx_sys_user_role_rid", "RoleId")]
public class TSysUserRole
{
	/// <summary>
	/// 用户ID
	/// </summary>
	[Column(Name = "user_id", IsPrimary = true)]
	public long UserId { get; set; }

	/// <summary>
	/// 角色ID
	/// </summary>
	[Column(Name = "role_id", IsPrimary = true)]
	public long RoleId { get; set; }
}