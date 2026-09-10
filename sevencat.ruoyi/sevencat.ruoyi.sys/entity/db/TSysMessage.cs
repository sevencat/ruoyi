using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.core.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 消息记录表 sys_message
/// </summary>
[Table(Name = "sys_message")]
[Index("idx_sys_message_category_time", "Category,CreateTime")]
public class TSysMessage : TBaseEntity
{
	/// <summary>
	/// 消息ID
	/// </summary>
	[Column(Name = "message_id", IsPrimary = true)]
	[Snowflake]
	public long MessageId { get; set; }

	/// <summary>
	/// 消息分组(system/notice/workflow)
	/// </summary>
	[Column(Name = "category", StringLength = 20, IsNullable = false)]
	public string Category { get; set; }

	/// <summary>
	/// 消息类型
	/// </summary>
	[Column(Name = "type", StringLength = 20, IsNullable = false)]
	public string Type { get; set; }

	/// <summary>
	/// 消息来源
	/// </summary>
	[Column(Name = "source", StringLength = 20, IsNullable = false)]
	public string Source { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	[Column(Name = "title", StringLength = 100, IsNullable = true)]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// 摘要消息
	/// </summary>
	[Column(Name = "message", StringLength = 500, IsNullable = true)]
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// 详细内容
	/// </summary>
	[Column(Name = "content", IsNullable = true)]
	public string Content { get; set; }

	/// <summary>
	/// 扩展数据JSON
	/// </summary>
	[Column(Name = "data_json", IsNullable = true)]
	public string DataJson { get; set; }

	/// <summary>
	/// 前端跳转路径
	/// </summary>
	[Column(Name = "path", StringLength = 500, IsNullable = true)]
	public string Path { get; set; }

	/// <summary>
	/// 目标用户ID串，0表示全局
	/// </summary>
	[Column(Name = "send_user_ids", StringLength = 2000, IsNullable = false)]
	public string SendUserIds { get; set; } = "0";
}