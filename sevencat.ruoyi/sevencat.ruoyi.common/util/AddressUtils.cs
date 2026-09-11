using System.Net;
using System.Net.Sockets;
using sevencat.common;

namespace sevencat.ruoyi.common.util;

/// <summary>
/// IP 归属地工具类（对应 Java 的 <c>org.dromara.common.core.utils.ip.AddressUtils</c>）。
/// </summary>
public static class AddressUtils
{
	/// <summary>
	/// 未知IP
	/// </summary>
	public const string UNKNOWN_IP = "XX XX";

	/// <summary>
	/// 内网地址
	/// </summary>
	public const string LOCAL_ADDRESS = "内网IP";

	/// <summary>
	/// 根据 IP 查询真实地址，内网地址不查询。
	/// </summary>
	/// <param name="ip">IP 地址</param>
	/// <returns>真实地址，非法 IP 或查询不到时返回 <see cref="UNKNOWN_IP"/></returns>
	public static string GetRealAddressByIP(string ip)
	{
		if (ip.IsNullOrWhiteSpace() || !IPAddress.TryParse(ip.Trim(), out var address))
		{
			return UNKNOWN_IP;
		}

		if (IsInnerIp(address))
		{
			return LOCAL_ADDRESS;
		}

		// TODO Java 端使用 ip2region 离线库查询归属地，C# 端暂无对应实现，公网地址先返回未知
		return UNKNOWN_IP;
	}

	/// <summary>
	/// 判断是否为内网/保留地址（含 IPv4 与 IPv6）。
	/// </summary>
	/// <param name="address">IP 地址</param>
	/// <returns>内网地址返回 true</returns>
	private static bool IsInnerIp(IPAddress address)
	{
		if (address.IsIPv4MappedToIPv6)
		{
			address = address.MapToIPv4();
		}

		if (address.AddressFamily == AddressFamily.InterNetwork)
		{
			var bytes = address.GetAddressBytes();
			return bytes[0] switch
			{
				0 or 10 or 127 => true,
				169 => bytes[1] == 254, // 169.254.0.0/16 链路本地
				172 => bytes[1] >= 16 && bytes[1] <= 31, // 172.16.0.0/12
				192 => bytes[1] == 168, // 192.168.0.0/16
				_ => false
			};
		}

		if (address.AddressFamily == AddressFamily.InterNetworkV6)
		{
			return IPAddress.IsLoopback(address)
			       || address.IsIPv6LinkLocal
			       || address.IsIPv6SiteLocal
			       || address.IsIPv6UniqueLocal;
		}

		return false;
	}
}
