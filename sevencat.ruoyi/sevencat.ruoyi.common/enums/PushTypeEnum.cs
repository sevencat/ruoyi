namespace sevencat.ruoyi.common.enums;

/// <summary>
/// 推送消息类型枚举
/// </summary>
public enum PushTypeEnum
{
	/// <summary>
	/// 通用消息
	/// </summary>
	Message,

	/// <summary>
	/// 通知公告
	/// </summary>
	Notice,

	/// <summary>
	/// 大模型消息
	/// </summary>
	Llm,

	/// <summary>
	/// 自定义消息
	/// </summary>
	Custom
}

/// <summary>
/// 推送消息类型枚举扩展
/// </summary>
public static class PushTypeEnumExtensions
{
	/// <summary>
	/// 获取消息类型标识
	/// </summary>
	public static string GetTypeValue(this PushTypeEnum type) => type switch
	{
		PushTypeEnum.Message => "message",
		PushTypeEnum.Notice => "notice",
		PushTypeEnum.Llm => "llm",
		PushTypeEnum.Custom => "custom",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
}
