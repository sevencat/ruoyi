namespace sevencat.ruoyi.common.excel.attr;

//自动忽略其他所有未加注解的字段
[AttributeUsage(AttributeTargets.Class)]
public class ExcelIgnoreUnannotatedAttribute : Attribute
{
}