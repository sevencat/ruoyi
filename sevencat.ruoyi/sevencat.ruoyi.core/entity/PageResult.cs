namespace sevencat.ruoyi.core.entity;

public class PageResult<T>
{
	public long Total { get; set; }
	public List<T> Rows { get; set; }
}