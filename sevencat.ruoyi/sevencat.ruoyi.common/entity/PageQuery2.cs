namespace sevencat.ruoyi.common.entity;

public class PageQuery2
{
	public int? PageSize { get; set; }

	public int? PageNum { get; set; }

	public string OrderByColumn { get; set; }

	public string IsAsc { get; set; }

	/**
     * 当前记录起始索引 默认值
     */
	public const int DEFAULT_PAGE_NUM = 1;

	/**
	 * 每页显示记录数 默认值 默认查全部
	 */
	public const int DEFAULT_PAGE_SIZE = int.MaxValue;

	public PageQuery2(int page, int pageSize = 20)
	{
		this.PageNum = page;
		this.PageSize = pageSize;
	}

	public PageQuery2()
	{
	}
}