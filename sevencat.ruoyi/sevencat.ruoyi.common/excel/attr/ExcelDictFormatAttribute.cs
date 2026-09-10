namespace sevencat.ruoyi.common.excel.attr;

/// <summary>
/// 字典格式化
/// <para>对应 Java 的 @ExcelDictFormat，用于声明字段值在 Excel 中的显示文本转换规则。</para>
/// <para>用法：<c>[ExcelDictFormat(ReadConverterExp = "0=男,1=女,2=未知")]</c> 或 <c>[ExcelDictFormat(DictType = "sys_user_gender")]</c>。</para>
/// </summary>
/// <remarks>
/// Java 原注解：
/// <code>
/// @Target({ElementType.FIELD})
/// @Retention(RetentionPolicy.RUNTIME)
/// @Inherited
/// public @interface ExcelDictFormat {
///     String dictType() default "";
///     String readConverterExp() default "";
///     String separator() default StringUtils.SEPARATOR; // ","
/// }
/// </code>
/// 对应关系：Java 的字段（FIELD）注解在 C# 中落到属性（Property）上；
/// 注解的「元素」在 C# 中即特性的公开可读写属性，默认值用属性初始值表达。
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ExcelDictFormatAttribute : Attribute
{
	/// <summary>
	/// 如果是字典类型，请设置字典的 type 值（如：sys_user_gender）
	/// </summary>
	public string DictType { get; set; } = string.Empty;

	/// <summary>
	/// 读取内容转表达式（如：0=男,1=女,2=未知）
	/// </summary>
	public string ReadConverterExp { get; set; } = string.Empty;

	/// <summary>
	/// 分隔符，读取字符串组内容
	/// </summary>
	public string Separator { get; set; } = ",";
}