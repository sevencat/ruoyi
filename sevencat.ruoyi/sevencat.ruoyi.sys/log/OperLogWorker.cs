using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.sys.log;

/// <summary>
/// 操作日志写库线程：从 <see cref="OperLogQueue"/> 取日志，补齐重活后落库。
/// </summary>
/// <remarks>
/// 对应 Java 端 <c>LogAspect</c> 中 <c>@Async</c> 的那一半：序列化参数/响应、查登录用户、IP 归属地、写库都在这里做，
/// 请求线程只负责采集入队。由 <c>AppModule</c> 通过 <c>AddHostedService</c> 注册，随应用启动/停止。
/// </remarks>
public class OperLogWorker(OperLogQueue queue, SysOperLogService operLogService, LoginService loginService)
	: BackgroundService
{
	private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 日志文本字段最大长度（<c>sys_oper_log</c> 中请求参数、返回参数、错误消息均为 VARCHAR(4000)）
	/// </summary>
	private const int MAX_TEXT_LENGTH = 4000;

	/// <summary>
	/// 日志中序列化请求/响应使用的 JSON 选项，与接口响应保持一致的命名与时间格式，
	/// 同时不转义中文，方便在日志页面直接阅读。
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		Converters =
		{
			new sevencat.common.json.JsonConverterUtil.DateTimeConverter(),
			new sevencat.common.json.JsonConverterUtil.DateTimeNullConverter()
		}
	};

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			await foreach (var item in queue.ReadAllAsync(stoppingToken))
			{
				await WriteAsync(item);
			}
		}
		catch (OperationCanceledException)
		{
			// 应用正在停止，属于正常流程
		}

		// 停机时把队列里剩余的日志写完，避免丢日志
		while (queue.TryDequeue(out var item))
		{
			await WriteAsync(item);
		}
	}

	/// <summary>
	/// 补齐后台字段并落库；失败只记日志，不影响其它日志
	/// </summary>
	/// <param name="item">队列元素</param>
	private async Task WriteAsync(OperLogItem item)
	{
		try
		{
			var operLog = item.OperLog;

			// 后台线程没有 HttpContext，只能按令牌从缓存取登录用户（未登录或令牌过期时字段留空）
			var loginUser = await loginService.GetLoginUserByToken(item.Token);
			if (loginUser != null)
			{
				operLog.OperName = loginUser.Username;
				operLog.UserId = loginUser.UserId;
				operLog.DeptId = loginUser.DeptId;
				operLog.DeptName = loginUser.DeptName;
				operLog.ClientKey = loginUser.ClientKey;
				operLog.DeviceType = loginUser.DeviceType;
				operLog.Browser = loginUser.Browser;
				operLog.Os = loginUser.Os;
			}

			if (operLog.OperIp.IsNotNullOrWhiteSpace())
			{
				operLog.OperLocation = AddressUtils.GetRealAddressByIP(operLog.OperIp);
			}

			if (item.SaveRequestData)
			{
				// 对应 Java 的 Arrays.toString(joinPoint.getArgs())
				operLog.OperParam = Truncate(ToJson(item.RequestArgs));
			}

			if (item.SaveResponseData)
			{
				operLog.JsonResult = Truncate(ToJson(item.ResultValue));
			}

			// 按 sys_oper_log 的列长度截断
			operLog.Title = Truncate(operLog.Title, 50);
			operLog.Method = Truncate(operLog.Method, 100);
			operLog.RequestMethod = Truncate(operLog.RequestMethod, 10);
			operLog.OperUrl = Truncate(operLog.OperUrl, 255);
			operLog.ErrorMsg = Truncate(operLog.ErrorMsg);

			await operLogService.RecordOperLog(operLog);
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "写操作日志失败:{0}", item.OperLog.Title);
		}
	}

	/// <summary>
	/// 序列化对象并剔除敏感字段（对应 Java 的 <c>PropertyPreExcludeFilter</c>）
	/// </summary>
	/// <param name="value">待序列化对象</param>
	/// <returns>JSON 字符串；value 为 null 时返回 null</returns>
	private static string ToJson(object value)
	{
		if (value == null)
		{
			return null;
		}

		var node = JsonSerializer.SerializeToNode(value, JsonOptions);
		if (node == null)
		{
			return null;
		}

		RemoveExcludedProperties(node);
		return node.ToJsonString(JsonOptions);
	}

	/// <summary>
	/// 递归剔除 <see cref="SystemConstants.EXCLUDE_PROPERTIES"/> 中的敏感字段（如 password）
	/// </summary>
	/// <param name="node">JSON 节点</param>
	private static void RemoveExcludedProperties(JsonNode node)
	{
		switch (node)
		{
			case JsonObject jsonObject:
				foreach (var name in jsonObject.Select(x => x.Key).ToList())
				{
					if (SystemConstants.EXCLUDE_PROPERTIES.Contains(name, StringComparer.OrdinalIgnoreCase))
					{
						jsonObject.Remove(name);
					}
					else
					{
						RemoveExcludedProperties(jsonObject[name]);
					}
				}

				break;
			case JsonArray jsonArray:
				foreach (var item in jsonArray)
				{
					RemoveExcludedProperties(item);
				}

				break;
		}
	}

	/// <summary>
	/// 按字段长度截断文本
	/// </summary>
	/// <param name="value">原始文本</param>
	/// <param name="maxLength">最大长度</param>
	/// <returns>截断后的文本</returns>
	private static string Truncate(string value, int maxLength = MAX_TEXT_LENGTH)
	{
		if (value == null || value.Length <= maxLength)
		{
			return value;
		}

		// 避免把代理对（emoji 等）截成非法字符，MySQL utf8mb4 会拒绝写入
		if (char.IsHighSurrogate(value[maxLength - 1]))
		{
			maxLength--;
		}

		return value[..maxLength];
	}
}
