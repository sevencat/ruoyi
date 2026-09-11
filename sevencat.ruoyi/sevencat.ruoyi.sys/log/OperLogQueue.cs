using System.Threading.Channels;
using Autofac.Annotation;
using sevencat.ruoyi.sys.bo;

namespace sevencat.ruoyi.sys.log;

/// <summary>
/// 操作日志内存队列：请求线程只入队，由 <see cref="OperLogWorker"/> 在后台线程出队写库。
/// </summary>
/// <remarks>
/// 必须是单例（项目默认 <c>SetDefaultAutofacScopeToSingleInstance</c>），否则每次解析都会拿到新队列，日志会丢失。
/// </remarks>
[Component]
public class OperLogQueue
{
	private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 队列容量：写库速度跟不上时宁可直接丢弃日志，也不拖慢接口
	/// </summary>
	private const int CAPACITY = 10000;

	private readonly Channel<OperLogItem> _channel = Channel.CreateBounded<OperLogItem>(
		new BoundedChannelOptions(CAPACITY)
		{
			// Wait 模式下队满时 TryWrite 返回 false，便于显式丢弃并告警（不会阻塞）
			FullMode = BoundedChannelFullMode.Wait,
			SingleReader = true,
			SingleWriter = false
		});

	/// <summary>
	/// 入队（同步、不阻塞、不抛异常）
	/// </summary>
	/// <param name="item">待落库的日志</param>
	/// <returns>true 入队成功；false 队列已满，本条日志被丢弃</returns>
	internal bool TryEnqueue(OperLogItem item)
	{
		if (_channel.Writer.TryWrite(item))
		{
			return true;
		}

		Logger.Warn("操作日志队列已满({0})，丢弃日志:{1}", CAPACITY, item.OperLog.Title);
		return false;
	}

	/// <summary>
	/// 非阻塞出队（后台线程使用，主要用于停机时写完残留日志）
	/// </summary>
	/// <param name="item">出队的日志</param>
	/// <returns>true 取到日志；false 队列为空</returns>
	internal bool TryDequeue(out OperLogItem item)
	{
		return _channel.Reader.TryRead(out item);
	}

	/// <summary>
	/// 异步读取队列（后台线程使用）
	/// </summary>
	/// <param name="cancellationToken">取消标记</param>
	/// <returns>日志异步流</returns>
	internal IAsyncEnumerable<OperLogItem> ReadAllAsync(CancellationToken cancellationToken)
	{
		return _channel.Reader.ReadAllAsync(cancellationToken);
	}
}

/// <summary>
/// 操作日志队列元素：只保留原始数据与对象引用，
/// 序列化、IP 归属地、查登录用户、写库等耗时操作全部留到后台线程。
/// </summary>
internal sealed class OperLogItem
{
	/// <summary>
	/// 日志主体（标题、业务类型、请求信息、状态、耗时由请求线程填好；
	/// 用户信息、归属地、参数、响应由后台线程补全）
	/// </summary>
	public SysOperLogBo OperLog { get; init; }

	/// <summary>
	/// 请求参数原始对象（对应 Java 的 <c>joinPoint.getArgs()</c>，序列化交给后台线程）
	/// </summary>
	public object RequestArgs { get; init; }

	/// <summary>
	/// 响应结果原始对象（序列化交给后台线程）
	/// </summary>
	public object ResultValue { get; init; }

	/// <summary>
	/// 访问令牌：后台线程没有 HttpContext，只能按令牌从缓存取登录用户
	/// </summary>
	public string Token { get; init; }

	/// <summary>
	/// 是否保存请求参数
	/// </summary>
	public bool SaveRequestData { get; init; }

	/// <summary>
	/// 是否保存响应参数
	/// </summary>
	public bool SaveResponseData { get; init; }
}
