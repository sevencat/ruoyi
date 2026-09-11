using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.sys.util;

/// <summary>
/// 操作日志记录切面（对应 Java 的 <c>LogAspect</c>）
/// </summary>
/// <remarks>
/// Java 端是「<c>@Log</c> 注解 + AOP 环绕通知」，C# 端改为「<see cref="LogAttribute"/> 特性 + 全局 Action 过滤器」：
/// 过滤器在 <c>Program.cs</c> 的 <c>AddControllers(options =&gt; options.Filters.Add&lt;OperLogFilter&gt;())</c>
/// 中全局注册，因此只要给 action 打上 <c>[Log]</c> 就会自动记录操作日志。
/// </remarks>
public class OperLogFilter(SysOperLogService operLogService, LoginService loginService) : IAsyncActionFilter
{
	/// <summary>
	/// 操作状态：正常
	/// </summary>
	private const int OPER_STATUS_SUCCESS = 0;

	/// <summary>
	/// 操作状态：异常
	/// </summary>
	private const int OPER_STATUS_FAIL = 1;

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

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var logAttr = context.ActionDescriptor.EndpointMetadata.OfType<LogAttribute>().FirstOrDefault();
		if (logAttr == null)
		{
			await next();
			return;
		}

		var operLog = await BuildOperLog(context, logAttr);
		var stopwatch = Stopwatch.StartNew();
		var executed = await next();
		stopwatch.Stop();

		if (executed.Exception != null && !executed.ExceptionHandled)
		{
			// 对应 Java 的 handleException：记录失败状态与错误消息，异常本身继续由全局异常中间件处理
			operLog.Status = OPER_STATUS_FAIL;
			operLog.ErrorMsg = Truncate(executed.Exception is BaseException baseException
				? baseException.BuildMessage()
				: executed.Exception.Message);
		}
		else
		{
			operLog.Status = OPER_STATUS_SUCCESS;
			if (logAttr.IsSaveResponseData)
			{
				// 直接序列化 action 的返回值，避免为读取响应流而替换 HttpResponse.Body
				operLog.JsonResult = Truncate(ToJson((executed.Result as ObjectResult)?.Value));
			}
		}

		operLog.CostTime = stopwatch.ElapsedMilliseconds;
		operLog.OperTime = DateTime.Now;
		await operLogService.RecordOperLog(operLog);
	}

	/// <summary>
	/// 构造操作日志（对应 Java 的 <c>LogAspect.doAfterReturning</c> 中填充字段的部分）
	/// </summary>
	/// <param name="context">action 执行上下文</param>
	/// <param name="logAttr">action 上的 <see cref="LogAttribute"/></param>
	/// <returns>操作日志</returns>
	private async Task<SysOperLogBo> BuildOperLog(ActionExecutingContext context, LogAttribute logAttr)
	{
		var httpctx = context.HttpContext;
		var loginUser = await loginService.GetLoginUser();
		var ip = ServletUtils.GetClientIp(httpctx);

		var operLog = new SysOperLogBo
		{
			Title = Truncate(logAttr.Title, 50),
			BusinessType = (int)logAttr.BusinessType,
			OperatorType = (int)logAttr.OperatorType,
			Method = Truncate(GetMethodName(context), 100),
			RequestMethod = Truncate(httpctx.Request.Method, 10),
			OperUrl = Truncate(httpctx.Request.Path.Value, 255),
			OperIp = ip,
			OperLocation = ip.IsNotNullOrWhiteSpace() ? AddressUtils.GetRealAddressByIP(ip) : null,
			// 对应 Java 的 LoginHelper.getLoginUser()，未登录时这些字段留空
			OperName = loginUser?.Username,
			UserId = loginUser?.UserId,
			DeptId = loginUser?.DeptId,
			DeptName = loginUser?.DeptName,
			ClientKey = loginUser?.ClientKey,
			DeviceType = loginUser?.DeviceType,
			Browser = loginUser?.Browser,
			Os = loginUser?.Os
		};

		if (logAttr.IsSaveRequestData)
		{
			// 对应 Java 的 Arrays.toString(joinPoint.getArgs())，已绑定好的参数直接序列化即可
			operLog.OperParam = Truncate(ToJson(context.ActionArguments));
		}

		return operLog;
	}

	/// <summary>
	/// 拼接「类名.方法名()」形式的方法名称（对应 Java 的 <c>className + "." + methodName + "()"</c>）
	/// </summary>
	/// <param name="context">action 执行上下文</param>
	/// <returns>方法名称</returns>
	private static string GetMethodName(ActionExecutingContext context)
	{
		if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
		{
			return $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.ActionName}()";
		}

		return context.ActionDescriptor.DisplayName;
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
