namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 消息记录视图对象 sys_message
/// </summary>
// Java 原注解 @AutoMapper(target = SysMessage.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysMessageVo
{
	/// <summary>
	/// 消息ID
	/// </summary>
	public long? MessageId { get; set; }

	/// <summary>
	/// 消息分组
	/// </summary>
	public string Category { get; set; }

	/// <summary>
	/// 消息类型
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// 消息来源
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// 摘要消息
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// 详细内容
	/// </summary>
	public string Content { get; set; }

	/// <summary>
	/// 扩展数据
	/// </summary>
	public object Data { get; set; }

	/// <summary>
	/// 前端跳转路径
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }
}
