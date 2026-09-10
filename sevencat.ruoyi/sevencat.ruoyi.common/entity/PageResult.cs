using MapsterMapper;

namespace sevencat.ruoyi.common.entity;

public class PageResult<T>
{
	public long Total { get; set; }
	public long PageSize { get; set; }
	public long PageNum { get; set; }
	public List<T> Rows { get; set; }

	public PageResult<E> MapTo<E>(IMapper mapper)
	{
		var ret = new PageResult<E>();
		ret.Total = Total;
		ret.PageNum = PageNum;
		ret.PageSize = PageSize;
		ret.Rows = Rows.Select(x => mapper.Map<E>(x)).ToList();
		return ret;
	}
}