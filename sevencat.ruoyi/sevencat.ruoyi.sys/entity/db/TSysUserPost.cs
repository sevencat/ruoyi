using FreeSql.DataAnnotations;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 用户与岗位关联表
/// </summary>
[Table(Name = "sys_user_post")]
[Index("idx_sys_user_post_pid", "PostId")]
public class TSysUserPost
{
	/// <summary>
	/// 用户ID
	/// </summary>
	[Column(Name = "user_id", IsPrimary = true)]
	public long UserId { get; set; }

	/// <summary>
	/// 岗位ID
	/// </summary>
	[Column(Name = "post_id", IsPrimary = true)]
	public long PostId { get; set; }
}
