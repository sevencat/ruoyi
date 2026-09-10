using System.Linq.Expressions;
using FreeSql;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.db.util;

public static class FSqlExt
{
	public static PageResult<TReturn> ToPage<T, TReturn>(this ISelect<T> source, PageQuery2 parm,
		Expression<Func<T, TReturn>> select)
	{
		var page = new PageResult<TReturn>();
		page.PageSize = parm.PageSize;
		page.PageNum = parm.PageNum;
		if (parm.IsAsc.IsNotNullOrEmpty())
		{
			source.OrderByPropertyName(parm.OrderByColumn, !parm.IsAsc.Contains("desc"));
		}

		page.Rows = source
			.Count(out var total)
			.Page(parm.PageNum, parm.PageSize)
			.ToList(select);
		page.Total = (int)total;
		return page;
	}

	public static async Task<PageResult<T>> ToPage<T>(this ISelect<T> source, PageQuery2 parm)
	{
		var page = new PageResult<T>();
		page.PageSize = parm.PageSize;
		page.PageNum = parm.PageNum;
		if (parm.IsAsc.IsNotNullOrEmpty())
		{
			source.OrderByPropertyName(parm.OrderByColumn, !parm.IsAsc.Contains("desc"));
		}

		page.Rows = await source
			.Count(out var total)
			.Page(parm.PageNum, parm.PageSize)
			.ToListAsync();
		page.Total = (int)total;
		return page;
	}
}