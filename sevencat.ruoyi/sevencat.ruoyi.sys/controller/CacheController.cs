using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 缓存监控（对应 Java 的 <c>CacheController</c>）
/// </summary>
/// <remarks>
/// Java 端通过 <c>RedissonConnectionFactory</c> 获取 <c>RedisConnection</c> 后调用 <c>commands()</c>；
/// C# 端直接注入 FreeRedis 的 <see cref="RedisClient"/>（等价于 Java 的 Redis 连接工厂）。
/// </remarks>
[ApiController]
[Route("/api/monitor/cache")]
public class CacheController(RedisClient redisClient)
{
	/// <summary>
	/// 命令统计项在 INFO 中的键前缀（对应 Java 的 <c>cmdstat_</c>）
	/// </summary>
	private const string CommandStatPrefix = "cmdstat_";

	/// <summary>
	/// 获取 Redis 缓存监控信息（对应 Java 的 <c>getInfo</c>）。
	/// </summary>
	/// <returns>Redis 信息、库大小与命令统计</returns>
	[SaCheckPermission("monitor:cache:list")]
	[HttpGet]
	public CommonResult<CacheListInfoVo> GetInfo()
	{
		// 对应 Java 的 connection.commands().info("commandstats")
		var commandStats = ParseInfo(redisClient.Info("commandstats"));
		var pieList = new List<Dictionary<string, string>>();
		foreach (var (key, property) in commandStats)
		{
			// 对应 Java 的 StringUtils.removeStart(key, "cmdstat_") 与
			// StringUtils.substringBetween(property, "calls=", ",usec")
			pieList.Add(new Dictionary<string, string>(2)
			{
				["name"] = RemoveStart(key, CommandStatPrefix),
				["value"] = SubstringBetween(property, "calls=", ",usec")
			});
		}

		return CommonResult.Success(new CacheListInfoVo
		{
			// 对应 Java 的 connection.commands().info()
			Info = ParseInfo(redisClient.Info()),
			// 对应 Java 的 connection.commands().dbSize()
			DbSize = redisClient.DbSize(),
			CommandStats = pieList
		});
	}

	/// <summary>
	/// 将 Redis <c>INFO</c> 命令返回的原始文本解析为键值字典（对应 Java 的 <c>Properties</c>）。
	/// </summary>
	/// <param name="info">INFO 命令返回的原始文本</param>
	/// <returns>键值字典；入参为空时返回空字典</returns>
	private static Dictionary<string, string> ParseInfo(string info)
	{
		var result = new Dictionary<string, string>();
		if (string.IsNullOrWhiteSpace(info))
		{
			return result;
		}

		foreach (var line in info.Split('\n'))
		{
			var trimmed = line.Trim();
			// 跳过空行与 "# Server" 之类的分节标题（对应 Properties 解析时忽略注释行）
			if (trimmed.Length == 0 || trimmed[0] == '#')
			{
				continue;
			}

			var idx = trimmed.IndexOf(':');
			if (idx <= 0)
			{
				continue;
			}

			result[trimmed[..idx]] = trimmed[(idx + 1)..];
		}

		return result;
	}

	/// <summary>
	/// 若 <paramref name="value"/> 以 <paramref name="prefix"/> 开头则去掉该前缀
	/// （对应 Java 的 <c>StringUtils.removeStart</c>）。
	/// </summary>
	/// <param name="value">源字符串</param>
	/// <param name="prefix">待移除的前缀</param>
	/// <returns>移除前缀后的字符串；不匹配时原样返回</returns>
	private static string RemoveStart(string value, string prefix)
	{
		if (value == null)
		{
			return null;
		}

		return value.StartsWith(prefix, StringComparison.Ordinal) ? value[prefix.Length..] : value;
	}

	/// <summary>
	/// 取 <paramref name="value"/> 中位于 <paramref name="open"/> 与 <paramref name="close"/> 之间的子串
	/// （对应 Java 的 <c>StringUtils.substringBetween</c>）。
	/// </summary>
	/// <param name="value">源字符串</param>
	/// <param name="open">起始标记</param>
	/// <param name="close">结束标记</param>
	/// <returns>标记之间的子串；未找到标记时返回 null</returns>
	private static string SubstringBetween(string value, string open, string close)
	{
		if (value == null)
		{
			return null;
		}

		var start = value.IndexOf(open, StringComparison.Ordinal);
		if (start < 0)
		{
			return null;
		}

		start += open.Length;
		var end = value.IndexOf(close, start, StringComparison.Ordinal);
		if (end < 0)
		{
			return null;
		}

		return value[start..end];
	}
}
