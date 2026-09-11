using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Annotation;
using sevencat.common;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.excel.attr;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.sys.excel;

/// <summary>
/// 按 <see cref="ExcelDictFormatAttribute"/> 批量做字典转换（对应 Java 的 <c>ExcelDictConvert</c> 转换器管线）
/// </summary>
[Component]
public class ExcelDictFormatConverter(SysDictDataService dictDataService)
{
	/// <summary>
	/// 类型到「带字典格式化特性的属性」的缓存
	/// </summary>
	private static readonly ConcurrentDictionary<Type, List<DictProperty>> PropertyCache = new();

	/// <summary>
	/// 导出方向转换：字段值 → Excel 中的字典标签
	/// </summary>
	/// <typeparam name="T">带 <see cref="ExcelDictFormatAttribute"/> 特性的类型</typeparam>
	/// <param name="rows">待转换的数据行（就地修改）</param>
	public async Task ToExcelData<T>(IEnumerable<T> rows)
	{
		var properties = GetDictProperties(typeof(T));
		if (properties.Count == 0)
		{
			return;
		}

		foreach (var property in properties)
		{
			var map = await GetDictMap(property.Attribute);
			foreach (var row in rows)
			{
				if (property.Property.GetValue(row) is string value && value.IsNotNullOrWhiteSpace())
				{
					property.Property.SetValue(row,
						ExcelDictConvert.ConvertByMap(value, map, property.Attribute.Separator));
				}
			}
		}
	}

	/// <summary>
	/// 导入方向转换：Excel 中的字典标签 → 字段值
	/// </summary>
	/// <typeparam name="T">带 <see cref="ExcelDictFormatAttribute"/> 特性的类型</typeparam>
	/// <param name="row">待转换的数据行（就地修改）</param>
	public async Task ToFieldValue<T>(T row)
	{
		var properties = GetDictProperties(typeof(T));
		if (properties.Count == 0)
		{
			return;
		}

		foreach (var property in properties)
		{
			var map = await GetDictMap(property.Attribute);
			if (property.Property.GetValue(row) is string value && value.IsNotNullOrWhiteSpace())
			{
				property.Property.SetValue(row,
					ExcelDictConvert.ReverseByMap(value, map, property.Attribute.Separator));
			}
		}
	}

	/// <summary>
	/// 取得某个特性对应的「字典值 → 字典标签」映射
	/// </summary>
	/// <param name="attribute">字典格式化特性</param>
	/// <returns>字典值到字典标签的映射</returns>
	private async Task<Dictionary<string, string>> GetDictMap(ExcelDictFormatAttribute attribute)
	{
		// readConverterExp 优先（对应 Java ExcelDictConvert 中基于表达式转换的分支）
		if (attribute.ReadConverterExp.IsNotNullOrWhiteSpace())
		{
			return ExcelDictConvert.ParseConverterExp(attribute.ReadConverterExp, attribute.Separator);
		}

		return await dictDataService.SelectDictLabelMap(attribute.DictType);
	}

	/// <summary>
	/// 反射取得类型上所有带字典格式化特性的属性
	/// </summary>
	/// <param name="type">目标类型</param>
	/// <returns>属性与特性的配对列表</returns>
	private static List<DictProperty> GetDictProperties(Type type)
	{
		return PropertyCache.GetOrAdd(type, t => t.GetProperties()
			.Select(property => new DictProperty(property, property.GetCustomAttribute<ExcelDictFormatAttribute>()))
			.Where(item => item.Attribute != null)
			.ToList());
	}

	/// <summary>
	/// 属性与字典格式化特性的配对
	/// </summary>
	/// <param name="Property">属性</param>
	/// <param name="Attribute">字典格式化特性</param>
	private sealed record DictProperty(PropertyInfo Property, ExcelDictFormatAttribute Attribute);
}
