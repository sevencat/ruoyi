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
	private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, StreamWriter>> _onlineUsers = new();

	public async Task RegisterClientAsync(long userId, string token, HttpContext context,
		CancellationToken cancellationToken)
	{
		// 1. 设置响应头
		context.Response.ContentType = "text/event-stream";
		context.Response.Headers.Append("Cache-Control", "no-cache");
		context.Response.Headers.Append("Connection", "keep-alive");

		// 2. 创建并保持写入器
		var writer = new StreamWriter(context.Response.Body);
		// 同一 token 重复连接时，先向旧连接发送被踢下线通知并关闭
		await KickOff(userId, token);

		var userTokens = _onlineUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, StreamWriter>());
		userTokens[token] = writer;

		// 发送一条连接成功的确认消息
		await WriteSseEventAsync(writer, "connected", $"欢迎上线，用户: {userId}");

		// 3. 阻塞当前请求，直到客户端断开连接
		var tcs = new TaskCompletionSource();
		using (cancellationToken.Register(() => tcs.TrySetResult()))
		{
			await tcs.Task;
		}

		// 4. 客户端断开后，清理缓存（仅当该 token 对应的仍是本次的 writer 时才移除）
		if (_onlineUsers.TryGetValue(userId, out var tokens) && tokens.TryGetValue(token, out var current) &&
		    ReferenceEquals(current, writer))
		{
			tokens.TryRemove(token, out _);
			if (tokens.IsEmpty)
			{
				_onlineUsers.TryRemove(userId, out _);
			}
		}

		await writer.DisposeAsync();
	}

	/// <summary>
	/// 向指定用户的所有在线连接发送消息
	/// </summary>
	public async Task SendToUserAsync(long userId, string data, string? @event = null)
	{
		if (!_onlineUsers.TryGetValue(userId, out var userTokens))
		{
			return;
		}

		foreach (var (token, writer) in userTokens)
		{
			try
			{
				await WriteSseEventAsync(writer, @event, data);
			}
			catch
			{
				// 写入失败说明连接已死，移除它
				userTokens.TryRemove(token, out _);
			}
		}
	}

	/// <summary>
	/// 向指定用户的指定 token 连接发送消息
	/// </summary>
	public async Task SendToTokenAsync(long userId, string token, string data, string? @event = null)
	{
		if (_onlineUsers.TryGetValue(userId, out var userTokens) && userTokens.TryGetValue(token, out var writer))
		{
			try
			{
				await WriteSseEventAsync(writer, @event, data);
			}
			catch
			{
				// 写入失败说明连接已死，移除它
				userTokens.TryRemove(token, out _);
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

	/// <summary>
	/// 被踢下线通知的数据内容（对应 Java 的 <c>MessageConstants.KICKED</c>）
	/// </summary>
	public const string KickedMessage = "kicked";

	/// <summary>
	/// 强退指定用户的指定连接：先发送被踢下线通知，再关闭数据流。
	/// </summary>
	/// <param name="userId">用户 id</param>
	/// <param name="token">连接对应的 token</param>
	/// <param name="data">通知数据，默认 <see cref="KickedMessage"/></param>
	/// <param name="event">通知事件名，默认 message</param>
	public async Task KickOff(long userId, string token, string data = KickedMessage, string @event = "message")
	{
		// 取出并移除目标连接，避免后续消息再写入该连接
		if (!_onlineUsers.TryGetValue(userId, out var userTokens) || !userTokens.TryRemove(token, out var writer))
		{
			return;
		}

		try
		{
			// 通知客户端已被踢下线
			await WriteSseEventAsync(writer, @event, data);
		}
		catch
		{
			// 旧连接可能已断开，忽略通知失败
		}
		finally
		{
			// 通知发送完毕后关闭数据流
			await writer.DisposeAsync();
		}

		// 该用户已无在线连接时，移除用户节点
		if (userTokens.IsEmpty)
		{
			_onlineUsers.TryRemove(userId, out _);
		}
	}
}