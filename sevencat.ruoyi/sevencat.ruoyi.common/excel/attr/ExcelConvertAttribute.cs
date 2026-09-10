namespace sevencat.ruoyi.common.excel.attr;

[AttributeUsage(AttributeTargets.Property)]
public class ExcelConvertAttribute : Attribute
{
	public Type Type { get; set; }

	public ExcelConvertAttribute(Type type)
	{
		this.Type = type;
	}
}