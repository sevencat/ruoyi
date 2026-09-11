using Microsoft.AspNetCore.Http;
using sevencat.common;

namespace sevencat.ruoyi.common.util;

/// <summary>
/// 请求上下文工具类（对应 Java 的 <c>org.dromara.common.core.utils.ServletUtils</c>，仅保留当前项目用到的部分）。
/// </summary>
public static class ServletUtils
{
	/// <summary>
	/// User-Agent 请求头名称
	/// </summary>
	public const string USER_AGENT_HEADER = "User-Agent";

	/// <summary>
	/// 依次尝试的客户端 IP 请求头（对应 Java 的 <c>getClientIPByHeader</c>）
	/// </summary>
	private static readonly string[] IpHeaders =
	[
		"X-Forwarded-For",
		"X-Real-IP",
		"Proxy-Client-IP",
		"WL-Proxy-Client-IP",
		"HTTP_CLIENT_IP",
		"HTTP_X_FORWARDED_FOR"
	];

	/// <summary>
	/// 获取客户端 IP 地址，优先读取代理头，取不到时回退到连接地址。
	/// </summary>
	/// <param name="context">当前请求上下文</param>
	/// <returns>客户端 IP 地址，无法获取时返回 null</returns>
	public static string GetClientIp(HttpContext context)
	{
		if (context == null)
		{
			return null;
		}

		foreach (var header in IpHeaders)
		{
			if (!context.Request.Headers.TryGetValue(header, out var values))
			{
				continue;
			}

			var ip = values.FirstOrDefault();
			if (ip.IsNullOrWhiteSpace() || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			// X-Forwarded-For 可能是 "客户端IP, 代理1, 代理2" 的形式，只取第一段
			var idx = ip.IndexOf(',');
			if (idx > 0)
			{
				ip = ip[..idx];
			}

			// 对应 Java 的 StringUtils.strip(ip, "[]")，去掉 IPv6 的方括号
			ip = ip.Trim().Trim('[', ']');
			if (ip.IsNotNullOrWhiteSpace())
			{
				return ip;
			}
		}

		var remote = context.Connection.RemoteIpAddress;
		if (remote == null)
		{
			return null;
		}

		// IPv4 映射地址（::ffff:127.0.0.1）统一还原为 IPv4 展示
		if (remote.IsIPv4MappedToIPv6)
		{
			remote = remote.MapToIPv4();
		}

		return remote.ToString();
	}

	/// <summary>
	/// 获取 User-Agent 请求头内容。
	/// </summary>
	/// <param name="context">当前请求上下文</param>
	/// <returns>User-Agent 内容，取不到时返回 null</returns>
	public static string GetUserAgent(HttpContext context)
	{
		if (context == null)
		{
			return null;
		}

		return context.Request.Headers.TryGetValue(USER_AGENT_HEADER, out var values)
			? values.ToString()
			: null;
	}
}
