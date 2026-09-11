using System.Collections.Concurrent;
using System.Text.Json;
using Autofac.Annotation;
using Microsoft.AspNetCore.Http;
using sevencat.common;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SseManager
{
	private static readonly JsonSerializerOptions jsonOptions = JsonSerializerOptions.Web;
	// 存储 UserId 和对应的客户端数据流写入器
	private readonly ConcurrentDictionary<long, StreamWriter> _onlineUsers = new();

	public async Task RegisterClientAsync(long userId, HttpContext context, CancellationToken cancellationToken)
	{
		// 1. 设置响应头
		context.Response.ContentType = "text/event-stream";
		context.Response.Headers.Append("Cache-Control", "no-cache");
		context.Response.Headers.Append("Connection", "keep-alive");

		// 2. 创建并保持写入器
		var writer = new StreamWriter(context.Response.Body);
		_onlineUsers[userId] = writer;

		// 发送一条连接成功的确认消息
		await WriteSseEventAsync(writer, "connected", $"欢迎上线，用户: {userId}");

		// 3. 阻塞当前请求，直到客户端断开连接
		var tcs = new TaskCompletionSource();
		using (cancellationToken.Register(() => tcs.TrySetResult()))
		{
			await tcs.Task;
		}

		// 4. 客户端断开后，清理缓存
		_onlineUsers.TryRemove(userId, out _);
		await writer.DisposeAsync();
	}

	public async Task SendToUserAsync(long userId, string data, string? @event = null)
	{
		if (_onlineUsers.TryGetValue(userId, out var writer))
		{
			try
			{
				await WriteSseEventAsync(writer, @event, data);
			}
			catch
			{
				// 写入失败说明连接已死，移除它
				_onlineUsers.TryRemove(userId, out _);
			}
		}
	}

	public async Task BroadcastAsync(object data, string @event = null)
	{
		var jstr = data.ToJson(jsonOptions);
		foreach (var userId in _onlineUsers.Keys)
		{
			await SendToUserAsync(userId, jstr, @event);
		}
	}

	public async Task BroadcastAsync2(string data, string @event = null)
	{
		foreach (var userId in _onlineUsers.Keys)
		{
			await SendToUserAsync(userId, data, @event);
		}
	}

	// 辅助方法：严格按照 SSE 的规范格式写入数据
	private async Task WriteSseEventAsync(StreamWriter writer, string @event, string data)
	{
		if (!string.IsNullOrEmpty(@event))
		{
			await writer.WriteAsync($"event: {@event}\n");
		}

		await writer.WriteAsync($"data: {data}\n\n");
		await writer.FlushAsync(); // 必须立即刷新缓冲区，否则前端收不到
	}
}