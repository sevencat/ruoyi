namespace sevencat.ruoyi.common.enums;

/// <summary>
/// 推送消息来源枚举
/// </summary>
public enum PushSourceEnum
{
	/// <summary>
	/// 后端系统消息
	/// </summary>
	Backend,

	/// <summary>
	/// 通知公告
	/// </summary>
	Notice,

	/// <summary>
	/// 工作流
	/// </summary>
	Workflow,

	/// <summary>
	/// 大模型
	/// </summary>
	Llm,

	/// <summary>
	/// 客户端消息
	/// </summary>
	Client
}

/// <summary>
/// 推送消息来源枚举扩展
/// </summary>
public static class PushSourceEnumExtensions
{
	/// <summary>
	/// 获取消息来源标识
	/// </summary>
	public static string GetSourceValue(this PushSourceEnum source) => source switch
	{
		PushSourceEnum.Backend => "backend",
		PushSourceEnum.Notice => "notice",
		PushSourceEnum.Workflow => "workflow",
		PushSourceEnum.Llm => "llm",
		PushSourceEnum.Client => "client",
		_ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
	};
}
