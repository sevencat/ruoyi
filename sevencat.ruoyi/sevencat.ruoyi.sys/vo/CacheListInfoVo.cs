namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 缓存监控列表信息视图对象（对应 Java 的 <c>CacheController.CacheListInfoVo</c>）
/// </summary>
/// <remarks>
/// Java 端 <c>info</c> 为 <c>java.util.Properties</c>、<c>commandStats</c> 为 <c>List&lt;Map&lt;String,String&gt;&gt;</c>；
/// C# 端以 <see cref="Dictionary{TKey,TValue}"/> 等价表示，字典键不会被 JSON 命名策略改写，
/// 因此键名（如 <c>redis_version</c>、<c>name</c>、<c>value</c>）与 Java 输出保持一致。
/// </remarks>
public class CacheListInfoVo
{
	/// <summary>
	/// Redis 服务端信息（对应 Java 的 <c>info</c>，来自 <c>INFO</c> 命令）
	/// </summary>
	public Dictionary<string, string> Info { get; set; }

	/// <summary>
	/// 当前数据库的 key 数量（对应 Java 的 <c>dbSize</c>，来自 <c>DBSIZE</c> 命令）
	/// </summary>
	public long DbSize { get; set; }

	/// <summary>
	/// 命令统计（对应 Java 的 <c>commandStats</c>，每项含 <c>name</c> / <c>value</c>）
	/// </summary>
	public List<Dictionary<string, string>> CommandStats { get; set; }
}
