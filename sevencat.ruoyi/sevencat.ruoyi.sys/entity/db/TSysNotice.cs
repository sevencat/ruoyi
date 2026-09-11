using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 通知公告表 sys_notice
/// </summary>
[Table(Name = "sys_notice")]
public class TSysNotice : TBaseEntity
{
	/// <summary>
	/// 公告ID
	/// </summary>
	[Column(Name = "notice_id", IsPrimary = true)]
	[Snowflake]
	public long NoticeId { get; set; }

	/// <summary>
	/// 公告标题
	/// </summary>
	[Column(Name = "notice_title", StringLength = 50, IsNullable = false)]
	public string NoticeTitle { get; set; }

	/// <summary>
	/// 公告类型（1通知 2公告）
	/// </summary>
	[Column(Name = "notice_type", StringLength = 1, IsNullable = false)]
	public string NoticeType { get; set; }

	/// <summary>
	/// 公告内容
	/// </summary>
	[Column(Name = "notice_content", IsNullable = true)]
	public string NoticeContent { get; set; }

	/// <summary>
	/// 公告状态（0正常 1关闭）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; } = "0";

	/// <summary>
	/// 备注
	/// </summary>
	[Column(Name = "remark", StringLength = 255, IsNullable = true, Position = -1)]
	public string Remark { get; set; }
}
