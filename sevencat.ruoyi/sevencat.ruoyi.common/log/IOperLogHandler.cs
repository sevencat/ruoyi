using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.ruoyi.common.log.attr;

namespace sevencat.ruoyi.common.log;

/// <summary>
/// 操作日志处理器（对应 Java 的 AOP 切面 <c>LogAspect</c>）
/// </summary>
/// <remarks>
/// <see cref="LogAttribute"/> 采集到操作上下文后调用本接口，实现放在 sys 层（依赖 FreeSql 与业务服务），
/// 由 sys 层决定「同步采集入队 + 后台线程写库」。
/// <para>实现约定：必须是同步、非阻塞的，只做采集与入队，不得做任何 IO（查缓存、序列化、写库等）。</para>
/// </remarks>
public interface IOperLogHandler
{
	/// <summary>
	/// 记录一条操作日志
	/// </summary>
	/// <param name="executingContext">action 执行前的上下文（提供请求参数、请求信息）</param>
	/// <param name="executedContext">action 执行后的上下文（提供返回值与异常）</param>
	/// <param name="log">当前 action 上的 <see cref="LogAttribute"/></param>
	/// <param name="costTime">action 执行耗时（毫秒）</param>
	void Record(ActionExecutingContext executingContext, ActionExecutedContext executedContext, LogAttribute log,
		long costTime);
}