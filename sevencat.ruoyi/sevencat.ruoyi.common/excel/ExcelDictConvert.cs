using System.Text;
using sevencat.common;

namespace sevencat.ruoyi.common.excel;

/// <summary>
/// 字典格式化转换处理（对应 Java 的 <c>ExcelDictConvert</c>）
/// </summary>
/// <remarks>
/// Java 通过 fesod 的转换器管线，在读写 Excel 时按字段上的
/// <c>@ExcelDictFormat(dictType / readConverterExp / separator)</c> 自动转换；
/// MiniExcel 没有转换器管线，这里提供等价的转换函数，由业务层在导入导出前后显式调用。
/// </remarks>
public static class ExcelDictConvert
{
	/// <summary>
	/// 字典值与字典标签之间的默认分隔符
	/// </summary>
	public const string SEPARATOR = ",";

	/// <summary>
	/// 解析 <c>readConverterExp</c> 表达式为 字典值 → 字典标签 映射
	/// </summary>
	/// <param name="converterExp">转换表达式，形如 <c>0=男,1=女,2=未知</c></param>
	/// <param name="separator">表达式内条目分隔符</param>
	/// <returns>字典值到字典标签的映射；表达式为空时返回空字典</returns>
	public static Dictionary<string, string> ParseConverterExp(string converterExp, string separator = SEPARATOR)
	{
		var map = new Dictionary<string, string>();
		if (converterExp.IsNullOrWhiteSpace())
		{
			return map;
		}

		foreach (var item in converterExp.Split(separator))
		{
			var itemArray = item.Split('=');
			if (itemArray.Length < 2 || itemArray[0].IsNullOrWhiteSpace())
			{
				continue;
			}

			map[itemArray[0]] = itemArray[1];
		}

		return map;
	}

	/// <summary>
	/// 导出方向转换：字典值 → 字典标签
	/// </summary>
	/// <param name="propertyValue">实体字段值，可用 <paramref name="separator"/> 分隔多个值</param>
	/// <param name="map">字典值到字典标签的映射</param>
	/// <param name="separator">多值分隔符</param>
	/// <returns>转换后的字典标签；无匹配时返回空字符串</returns>
	public static string ConvertByMap(string propertyValue, IDictionary<string, string> map,
		string separator = SEPARATOR)
	{
		if (propertyValue.IsNullOrWhiteSpace() || map == null || map.Count == 0)
		{
			return string.Empty;
		}

		if (!propertyValue.Contains(separator))
		{
			return map.TryGetValue(propertyValue, out var label) ? label : string.Empty;
		}

		var result = new StringBuilder();
		foreach (var value in propertyValue.Split(separator))
		{
			if (map.TryGetValue(value, out var item))
			{
				result.Append(item).Append(separator);
			}
		}

		return result.ToString().TrimEnd(separator[0]);
	}

	/// <summary>
	/// 导入方向转换：字典标签 → 字典值
	/// </summary>
	/// <param name="propertyValue">Excel 中的字典标签，可用 <paramref name="separator"/> 分隔多个值</param>
	/// <param name="map">字典值到字典标签的映射</param>
	/// <param name="separator">多值分隔符</param>
	/// <returns>转换后的字典值；无匹配时返回空字符串</returns>
	public static string ReverseByMap(string propertyValue, IDictionary<string, string> map,
		string separator = SEPARATOR)
	{
		if (propertyValue.IsNullOrWhiteSpace() || map == null || map.Count == 0)
		{
			return string.Empty;
		}

		if (!propertyValue.Contains(separator))
		{
			return FindKey(map, propertyValue);
		}

		var result = new StringBuilder();
		foreach (var label in propertyValue.Split(separator))
		{
			var value = FindKey(map, label);
			if (value.IsNotNullOrWhiteSpace())
			{
				result.Append(value).Append(separator);
			}
		}

		return result.ToString().TrimEnd(separator[0]);
	}

	/// <summary>
	/// 导出方向转换：按 <c>readConverterExp</c> 表达式把字典值转换为字典标签
	/// </summary>
	/// <param name="propertyValue">实体字段值</param>
	/// <param name="converterExp">转换表达式，形如 <c>0=男,1=女,2=未知</c></param>
	/// <param name="separator">表达式内条目分隔符</param>
	/// <returns>转换后的字典标签</returns>
	public static string ConvertByExp(string propertyValue, string converterExp, string separator = SEPARATOR)
	{
		return ConvertByMap(propertyValue, ParseConverterExp(converterExp, separator), separator);
	}

	/// <summary>
	/// 导入方向转换：按 <c>readConverterExp</c> 表达式把字典标签转换为字典值
	/// </summary>
	/// <param name="propertyValue">Excel 中的字典标签</param>
	/// <param name="converterExp">转换表达式，形如 <c>0=男,1=女,2=未知</c></param>
	/// <param name="separator">表达式内条目分隔符</param>
	/// <returns>转换后的字典值</returns>
	public static string ReverseByExp(string propertyValue, string converterExp, string separator = SEPARATOR)
	{
		return ReverseByMap(propertyValue, ParseConverterExp(converterExp, separator), separator);
	}

	/// <summary>
	/// 按字典标签反查字典值，标签重复时取排序靠前的一个
	/// </summary>
	/// <param name="map">字典值到字典标签的映射</param>
	/// <param name="label">字典标签</param>
	/// <returns>字典值；无匹配时返回空字符串</returns>
	private static string FindKey(IDictionary<string, string> map, string label)
	{
		foreach (var pair in map)
		{
			if (pair.Value == label)
			{
				return pair.Key;
			}
		}

		return string.Empty;
	}
}
