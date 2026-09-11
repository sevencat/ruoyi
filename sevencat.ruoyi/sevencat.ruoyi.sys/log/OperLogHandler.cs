using Autofac.Annotation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.log;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.sys.log;

/// <summary>
/// 操作日志处理器（对应 Java 的 <c>LogAspect</c>）：只采集 + 入队，写库交给 <see cref="OperLogWorker"/>。
/// </summary>
/// <remarks>
/// <para>运行在当前请求线程，必须是同步、非阻塞的：查登录用户（缓存）、IP 归属地、JSON 序列化、
/// 写库等下放给后台线程完成，接口响应时间不受日志影响。</para>
/// <para>字段长度按 <c>sys_oper_log</c> 列长度截断，统一放在 <see cref="OperLogWorker"/> 落库前处理。</para>
/// </remarks>
[Component]
public class OperLogHandler(OperLogQueue queue, LoginService loginService) : IOperLogHandler
{
	/// <summary>
	/// 操作状态：正常
	/// </summary>
	private const int OPER_STATUS_SUCCESS = 0;

	/// <summary>
	/// 操作状态：异常
	/// </summary>
	private const int OPER_STATUS_FAIL = 1;

	public void Record(ActionExecutingContext executingContext, ActionExecutedContext executedContext, LogAttribute log,
		long costTime)
	{
		var httpctx = executingContext.HttpContext;
		var operLog = new SysOperLogBo
		{
			Title = log.Title,
			BusinessType = (int)log.BusinessType,
			OperatorType = (int)log.OperatorType,
			Method = GetMethodName(executingContext),
			RequestMethod = httpctx.Request.Method,
			OperUrl = httpctx.Request.Path.Value,
			OperIp = ServletUtils.GetClientIp(httpctx),
			OperTime = DateTime.Now,
			CostTime = costTime,
			Status = OPER_STATUS_SUCCESS
		};

		if (executedContext.Exception != null && !executedContext.ExceptionHandled)
		{
			// 对应 Java 的 handleException：记录失败状态与错误消息，异常本身继续由全局异常中间件处理
			operLog.Status = OPER_STATUS_FAIL;
			operLog.ErrorMsg = executedContext.Exception is BaseException baseException
				? baseException.BuildMessage()
				: executedContext.Exception.Message;
		}

		queue.TryEnqueue(new OperLogItem
		{
			OperLog = operLog,
			// 参数与响应只传原始对象引用，序列化留到后台线程
			RequestArgs = log.IsSaveRequestData ? executingContext.ActionArguments : null,
			ResultValue = log.IsSaveResponseData ? (executedContext.Result as ObjectResult)?.Value : null,
			// 登录用户也留到后台线程按令牌查缓存，避免请求线程做 IO
			Token = loginService.TryGetToken(httpctx),
			SaveRequestData = log.IsSaveRequestData,
			SaveResponseData = log.IsSaveResponseData
		});
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
}
