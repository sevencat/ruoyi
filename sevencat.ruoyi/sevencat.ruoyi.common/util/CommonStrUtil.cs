using System.Text;

namespace sevencat.ruoyi.common.util;

public static class CommonStrUtil
{
	public const string COLON = ":";

	public const string SEPARATOR = ",";

	public const string SLASH = "/";

	/// <summary>
	/// 将字符串首字符转为标题格式（首字母大写），其余字符保持不变。
	/// 对应 Java commons-lang3 的 <c>StringUtils.capitalize(String)</c>。
	/// </summary>
	/// <param name="str">待处理字符串，可为 null。</param>
	/// <returns>
	/// 首字符大写后的字符串；若 <paramref name="str"/> 为 null 或空串，则原样返回。
	/// </returns>
	public static string Capitalize(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}

		// 取首个码点，兼容代理对（对齐 Java 按 codepoint 处理的方式）
		var firstRune = str.EnumerateRunes().First();
		// Java 用的是 Character.toTitleCase，绝大多数情况下与 ToUpperInvariant 等价
		var newRune = Rune.ToUpperInvariant(firstRune);
		if (newRune == firstRune)
		{
			// 首字符已是大写，直接返回原串
			return str;
		}

		return newRune.ToString() + str[firstRune.Utf16SequenceLength..];
	}

	/// <summary>
	/// 判断字符串是否为 http/https 链接。
	/// 对应 Java <c>StringUtils.ishttp(String)</c>（RuoYi 中用于外链地址校验）。
	/// </summary>
	/// <param name="str">待判断字符串。</param>
	/// <returns>以 http:// 或 https:// 开头返回 true，否则返回 false（含 null / 空串）。</returns>
	public static bool Ishttp(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}

		return str.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
		       || str.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// 依次将 <paramref name="text"/> 中出现的 <paramref name="searchList"/> 子串替换为
	/// <paramref name="replacementList"/> 中对应位置的子串（不递归重复替换）。
	/// 对应 Java commons-lang3 的 <c>StringUtils.replaceEach(String, String[], String[])</c>。
	/// </summary>
	/// <param name="text">待替换的源字符串。</param>
	/// <param name="searchList">待查找的子串数组；元素为 null 或空串时跳过。</param>
	/// <param name="replacementList">替换子串数组，与 <paramref name="searchList"/> 一一对应；元素为 null 时跳过。</param>
	/// <returns>替换后的字符串；无匹配或 <paramref name="text"/> 为 null/空串时返回原值。</returns>
	/// <exception cref="ArgumentException">两个数组长度不一致时抛出。</exception>
	public static string ReplaceEach(string text, string[] searchList, string[] replacementList)
	{
		if (searchList == null || replacementList == null)
		{
			return text;
		}

		if (searchList.Length != replacementList.Length)
		{
			throw new ArgumentException(
				$"Search and Replace array lengths don't match: {searchList.Length} vs {replacementList.Length}");
		}

		if (string.IsNullOrEmpty(text) || searchList.Length == 0)
		{
			return text;
		}

		// 记录某个查找串是否已确定不再出现，避免重复查找
		var noMoreMatchesForReplIndex = new bool[searchList.Length];

		int textIndex = -1;
		int replaceIndex = -1;
		int tempIndex;

		// 找出最先出现（索引最小）的那个查找串
		for (int i = 0; i < searchList.Length; i++)
		{
			if (noMoreMatchesForReplIndex[i] || string.IsNullOrEmpty(searchList[i]) || replacementList[i] == null)
			{
				continue;
			}

			tempIndex = text.IndexOf(searchList[i]!, StringComparison.Ordinal);

			if (tempIndex == -1)
			{
				noMoreMatchesForReplIndex[i] = true;
			}
			else if (textIndex == -1 || tempIndex < textIndex)
			{
				textIndex = tempIndex;
				replaceIndex = i;
			}
		}

		// 没有任何匹配，直接返回原串
		if (textIndex == -1)
		{
			return text;
		}

		int start = 0;
		var buf = new StringBuilder(text.Length);

		while (textIndex != -1)
		{
			for (int i = start; i < textIndex; i++)
			{
				buf.Append(text[i]);
			}

			buf.Append(replacementList[replaceIndex]!);

			start = textIndex + searchList[replaceIndex]!.Length;

			textIndex = -1;
			replaceIndex = -1;

			// 从 start 之后继续寻找下一个最早出现的匹配
			for (int i = 0; i < searchList.Length; i++)
			{
				if (noMoreMatchesForReplIndex[i] || string.IsNullOrEmpty(searchList[i]) || replacementList[i] == null)
				{
					continue;
				}

				tempIndex = text.IndexOf(searchList[i]!, start, StringComparison.Ordinal);

				if (tempIndex == -1)
				{
					noMoreMatchesForReplIndex[i] = true;
				}
				else if (textIndex == -1 || tempIndex < textIndex)
				{
					textIndex = tempIndex;
					replaceIndex = i;
				}
			}
		}

		for (int i = start; i < text.Length; i++)
		{
			buf.Append(text[i]);
		}

		return buf.ToString();
	}

	/// <summary>
	/// 把 <paramref name="text"/> 中第一处出现的 <paramref name="search"/> 替换为
	/// <paramref name="replacement"/>（只替换一次，不递归）。
	/// 对应 Java commons-lang3 的 <c>StringUtils.replaceOnce(String, String, String)</c>。
	/// </summary>
	/// <param name="text">待替换的源字符串。</param>
	/// <param name="search">待查找的子串；为 null 或空串时不替换。</param>
	/// <param name="replacement">替换子串。</param>
	/// <returns>替换后的字符串；无匹配或 <paramref name="text"/> 为 null/空串时返回原值。</returns>
	public static string ReplaceOnce(string text, string search, string replacement)
	{
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
		{
			return text;
		}

		var index = text.IndexOf(search!, StringComparison.Ordinal);
		if (index < 0)
		{
			return text;
		}

		var buf = new StringBuilder(text.Length - search!.Length + (replacement?.Length ?? 0));
		buf.Append(text, 0, index);
		buf.Append(replacement);
		buf.Append(text, index + search.Length, text.Length - index - search.Length);
		return buf.ToString();
	}
}