using sevencat.common;
using UAParser;

namespace sevencat.ruoyi.sys.util;

/// <summary>
/// 浏览器/操作系统解析工具类（对应 Java 中 <c>cn.hutool.http.useragent.UserAgentUtil</c> 的用法）。
/// </summary>
/// <remarks>
/// UAParser 构造时会解析内嵌规则并编译正则，开销较大，故只创建一次并复用。
/// </remarks>
public static class UserAgentUtils
{
	/// <summary>
	/// 默认 UA 解析器
	/// </summary>
	private static readonly Parser UaParser = Parser.GetDefault();

	/// <summary>
	/// 解析 User-Agent 字符串，提取浏览器与操作系统名称。
	/// </summary>
	/// <param name="userAgent">User-Agent 请求头内容</param>
	/// <returns>浏览器名称与操作系统名称，无法解析时返回空串</returns>
	public static (string Browser, string Os) Parse(string userAgent)
	{
		if (userAgent.IsNullOrWhiteSpace())
		{
			return (string.Empty, string.Empty);
		}

		try
		{
			var client = UaParser.Parse(userAgent);
			return (client.UA.Family, client.OS.Family);
		}
		catch (Exception)
		{
			// 解析异常不应影响登录流程
			return (string.Empty, string.Empty);
		}
	}
}
