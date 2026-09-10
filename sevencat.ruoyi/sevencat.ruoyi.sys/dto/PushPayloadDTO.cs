using sevencat.ruoyi.common.enums;

namespace sevencat.ruoyi.sys.dto;

/// <summary>
/// 推送给前端的统一消息体
/// </summary>
public class PushPayloadDTO
{
	/// <summary>
	/// 消息记录ID
	/// </summary>
	public long? MessageId { get; set; }

	/// <summary>
	/// 消息类型
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// 消息来源
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// 文本消息
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// 扩展数据
	/// </summary>
	public object Data { get; set; }

	/// <summary>
	/// 前端跳转路径
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// 时间戳
	/// </summary>
	public long? Timestamp { get; set; }

	/// <summary>
	/// 构建推送消息体，缺省消息类型与来源时使用系统默认值
	/// </summary>
	/// <param name="type">消息类型</param>
	/// <param name="source">消息来源</param>
	/// <param name="message">文本消息</param>
	/// <param name="data">扩展数据</param>
	/// <returns>推送消息体</returns>
	public static PushPayloadDTO Of(string type, string source, string message, object data)
	{
		return new PushPayloadDTO
		{
			Type = string.IsNullOrWhiteSpace(type) ? PushTypeEnum.Message.GetTypeValue() : type,
			Source = string.IsNullOrWhiteSpace(source) ? PushSourceEnum.Backend.GetSourceValue() : source,
			Message = message,
			Data = data,
			Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};
	}

	/// <summary>
	/// 通过枚举值构建推送消息体
	/// </summary>
	/// <param name="type">消息类型枚举</param>
	/// <param name="source">消息来源枚举</param>
	/// <param name="message">文本消息</param>
	/// <param name="data">扩展数据</param>
	/// <returns>推送消息体</returns>
	public static PushPayloadDTO Of(PushTypeEnum? type, PushSourceEnum? source, string message, object data)
	{
		return Of(type?.GetTypeValue(), source?.GetSourceValue(), message, data);
	}

	/// <summary>
	/// 构建带前端跳转路径的推送消息体
	/// </summary>
	/// <param name="type">消息类型枚举</param>
	/// <param name="source">消息来源枚举</param>
	/// <param name="message">文本消息</param>
	/// <param name="data">扩展数据</param>
	/// <param name="path">前端跳转路径</param>
	/// <returns>推送消息体</returns>
	public static PushPayloadDTO Of(PushTypeEnum? type, PushSourceEnum? source, string message, object data, string path)
	{
		var payload = Of(type, source, message, data);
		payload.Path = path;
		return payload;
	}
}
