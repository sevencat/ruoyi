using System.Diagnostics;
using Autofac.Util;
using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.ruoyi.common.enums;

namespace sevencat.ruoyi.common.log.attr;

/// <summary>
/// 操作日志记录特性（对应 Java 的 <c>com.ruoyi.common.annotation.Log</c>）
/// </summary>
/// <remarks>
/// Java 端是「<c>@Log</c> 注解 + AOP 环绕通知」；C# 端没有注解切面，改为「特性即过滤器」：
/// 本类型继承 <see cref="ActionFilterAttribute"/>，自身参与 action 执行，采集完成后把上下文交给
/// <see cref="IOperLogHandler"/>（由 sys 层实现），由其投递到后台队列写库，请求线程不做任何 IO。
/// <para>用法：<c>[Log("岗位管理", BusinessTypeEnum.Insert)]</c></para>
/// <para>注意：特性实例由框架在启动期创建并跨请求复用，因此所有 per-request 数据只能放
/// <c>HttpContext.Items</c>，不能放实例字段。</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class LogAttribute : ActionFilterAttribute
{
	/// <summary>
	/// 暂存 <see cref="ActionExecutingContext"/> 的 Items 键（用对象作键，避免与其他组件撞名）
	/// </summary>
	private static readonly object ExecutingContextKey = new();

	/// <summary>
	/// 暂存计时器的 Items 键
	/// </summary>
	private static readonly object StopwatchKey = new();

	/// <summary>
	/// 延迟解析日志处理器：特性实例由框架创建，无法构造注入，只能从 IoC 取（与 SaCheckPermissionAttribute 一致）
	/// </summary>
	private static readonly Lazy<IOperLogHandler> Handler = IocFactory.CreateLazy<IOperLogHandler>();

	/// <summary>
	/// 构造操作日志特性
	/// </summary>
	/// <param name="title">模块标题</param>
	/// <param name="businessType">业务类型</param>
	public LogAttribute(string title, BusinessTypeEnum businessType = BusinessTypeEnum.Other)
	{
		Title = title;
		BusinessType = businessType;
		// 与改造前的全局过滤器保持一致：日志切面处于最外层，权限校验不通过时也能记录
		Order = -1;
	}

	/// <summary>
	/// 模块标题
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// 业务类型
	/// </summary>
	public BusinessTypeEnum BusinessType { get; }

	/// <summary>
	/// 操作人类别
	/// </summary>
	public OperatorTypeEnum OperatorType { get; set; } = OperatorTypeEnum.Manage;

	/// <summary>
	/// 是否保存请求的参数
	/// </summary>
	public bool IsSaveRequestData { get; set; } = true;

	/// <summary>
	/// 是否保存响应的参数
	/// </summary>
	public bool IsSaveResponseData { get; set; } = true;

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		// 请求参数只挂在 ActionExecutingContext 上，先暂存给 OnActionExecuted 用
		var items = context.HttpContext.Items;
		items[ExecutingContextKey] = context;
		items[StopwatchKey] = Stopwatch.StartNew();
	}

	public override void OnActionExecuted(ActionExecutedContext context)
	{
		var items = context.HttpContext.Items;
		if (items[ExecutingContextKey] is not ActionExecutingContext executing)
		{
			// 没有采集过（例如内层过滤器已短路），不记录
			return;
		}

		items.Remove(ExecutingContextKey);
		var stopwatch = items[StopwatchKey] as Stopwatch;
		items.Remove(StopwatchKey);
		stopwatch?.Stop();

		var handler = Handler.Value;
		if (handler == null)
		{
			return;
		}

		// 同步调用：实现只应采集 + 入队，不阻塞、不写库
		handler.Record(executing, context, this, stopwatch?.ElapsedMilliseconds ?? 0);
	}
}
