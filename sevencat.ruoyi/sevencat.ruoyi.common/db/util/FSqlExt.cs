using System.Linq.Expressions;
using System.Reflection;
using FreeSql;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.db.util;

public static class FSqlExt
{
	/// <summary>
	/// <c>string.Contains(string)</c>，供手工构造模糊查询表达式使用
	/// </summary>
	private static readonly MethodInfo StringContainsMethod =
		typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);

	public static PageResult<TReturn> ToPage<T, TReturn>(this ISelect<T> source, PageQuery2 parm,
		Expression<Func<T, TReturn>> select)
	{
		var page = new PageResult<TReturn>();
		page.PageSize = parm.PageSize ?? 1;
		page.PageNum = parm.PageNum ?? 20;
		if (parm.IsAsc.IsNotNullOrEmpty())
		{
			source.OrderByPropertyName(parm.OrderByColumn, !parm.IsAsc.Contains("desc"));
		}

		page.Rows = source
			.Count(out var total)
			.Page(parm.PageNum ?? 20, parm.PageSize ?? 1)
			.ToList(select);
		page.Total = (int)total;
		return page;
	}

	public static async Task<PageResult<T>> ToPage<T>(this ISelect<T> source, PageQuery2 parm)
	{
		var page = new PageResult<T>();
		page.PageSize = parm.PageSize ?? 1;
		page.PageNum = parm.PageNum ?? 20;
		if (parm.IsAsc.IsNotNullOrEmpty())
		{
			source.OrderByPropertyName(parm.OrderByColumn, !parm.IsAsc.Contains("desc"));
		}

		page.Rows = await source
			.Count(out var total)
			.Page(parm.PageNum??20, parm.PageSize??1)
			.ToListAsync();
		page.Total = (int)total;
		return page;
	}

	/// <summary>
	/// 按请求参数中的时间区间追加检索条件，对应 Java RuoYi 的 <c>params.beginTime</c> / <c>params.endTime</c>。
	/// 两个参数都为空时不追加任何条件，只传一个时只追加对应的一侧。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <param name="source">查询对象</param>
	/// <param name="parameters">请求参数，通常为 BO 的 Params</param>
	/// <param name="field">参与比较的时间字段，如 <c>x =&gt; x.CreateTime</c></param>
	/// <param name="beginKey">起始时间参数名</param>
	/// <param name="endKey">结束时间参数名</param>
	/// <returns>追加条件后的查询对象</returns>
	public static ISelect<T> WhereTimeRange<T>(this ISelect<T> source,
		IDictionary<string, object> parameters,
		Expression<Func<T, DateTime?>> field,
		string beginKey = "beginTime",
		string endKey = "endTime")
	{
		var beginTime = GetDateTime(parameters, beginKey);
		if (beginTime.HasValue)
		{
			source = source.Where(BuildTimeCompare(field, beginTime.Value, true));
		}

		var endTime = GetDateTime(parameters, endKey);
		if (endTime.HasValue)
		{
			source = source.Where(BuildTimeCompare(field, endTime.Value, false));
		}

		return source;
	}

	/// <summary>
	/// 字符串非空白时追加模糊匹配条件，等价于
	/// <c>source.WhereIf(value.IsNotNullOrWhiteSpace(), x =&gt; x.Field.Contains(value))</c>。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <param name="source">查询对象</param>
	/// <param name="value">模糊匹配值，为 null / 空串 / 空白时不追加条件</param>
	/// <param name="field">参与匹配的字符串字段，如 <c>x =&gt; x.UserName</c></param>
	/// <returns>追加条件后的查询对象</returns>
	public static ISelect<T> WhereLike<T>(this ISelect<T> source, string value, Expression<Func<T, string>> field)
	{
		if (!value.IsNotNullOrWhiteSpace())
		{
			return source;
		}

		var body = Expression.Call(field.Body, StringContainsMethod, Expression.Constant(value));
		return source.Where(Expression.Lambda<Func<T, bool>>(body, field.Parameters));
	}

	/// <summary>
	/// 字符串非空白时追加等值条件，等价于
	/// <c>source.WhereIf(value.IsNotNullOrWhiteSpace(), x =&gt; x.Field == value)</c>。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <param name="source">查询对象</param>
	/// <param name="value">等值匹配值，为 null / 空串 / 空白时不追加条件</param>
	/// <param name="field">参与匹配的字符串字段，如 <c>x =&gt; x.Status</c></param>
	/// <returns>追加条件后的查询对象</returns>
	public static ISelect<T> WhereHasTextEq<T>(this ISelect<T> source, string value,
		Expression<Func<T, string>> field)
	{
		if (!value.IsNotNullOrWhiteSpace())
		{
			return source;
		}

		var body = Expression.Equal(field.Body, Expression.Constant(value));
		return source.Where(Expression.Lambda<Func<T, bool>>(body, field.Parameters));
	}

	/// <summary>
	/// 可空值非 null 时追加等值条件，等价于
	/// <c>source.WhereIf(value.HasValue, x =&gt; x.Field == value)</c>。适用于实体字段为不可空值类型的情况。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <typeparam name="TValue">值类型</typeparam>
	/// <param name="source">查询对象</param>
	/// <param name="value">等值匹配值，为 null 时不追加条件</param>
	/// <param name="field">参与匹配的字段，如 <c>x =&gt; x.UserId</c></param>
	/// <returns>追加条件后的查询对象</returns>
	public static ISelect<T> WhereNotNullEq<T, TValue>(this ISelect<T> source, TValue? value,
		Expression<Func<T, TValue>> field)
		where TValue : struct
	{
		if (!value.HasValue)
		{
			return source;
		}

		var body = Expression.Equal(field.Body, Expression.Constant(value.Value, typeof(TValue)));
		return source.Where(Expression.Lambda<Func<T, bool>>(body, field.Parameters));
	}

	/// <summary>
	/// 可空值非 null 时追加等值条件，等价于
	/// <c>source.WhereIf(value.HasValue, x =&gt; x.Field == value)</c>。适用于实体字段为可空值类型的情况。
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <typeparam name="TValue">值类型</typeparam>
	/// <param name="source">查询对象</param>
	/// <param name="value">等值匹配值，为 null 时不追加条件</param>
	/// <param name="field">参与匹配的可空字段，如 <c>x =&gt; x.DeptId</c></param>
	/// <returns>追加条件后的查询对象</returns>
	public static ISelect<T> WhereNotNullEq<T, TValue>(this ISelect<T> source, TValue? value,
		Expression<Func<T, TValue?>> field)
		where TValue : struct
	{
		if (!value.HasValue)
		{
			return source;
		}

		var body = Expression.Equal(field.Body, Expression.Constant(value.Value, typeof(TValue?)));
		return source.Where(Expression.Lambda<Func<T, bool>>(body, field.Parameters));
	}

	/// <summary>
	/// 构造 <c>field &gt;= value</c> 或 <c>field &lt;= value</c> 的查询表达式
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	/// <param name="field">参与比较的时间字段</param>
	/// <param name="value">比较值</param>
	/// <param name="greaterOrEqual">true 为 &gt;=，false 为 &lt;=</param>
	/// <returns>查询表达式</returns>
	private static Expression<Func<T, bool>> BuildTimeCompare<T>(Expression<Func<T, DateTime?>> field,
		DateTime value, bool greaterOrEqual)
	{
		// 右侧常量显式声明为 DateTime?，与字段类型一致，生成的表达式树等价于手写 x.CreateTime >= beginTime
		var right = Expression.Constant(value, typeof(DateTime?));
		var body = greaterOrEqual
			? Expression.GreaterThanOrEqual(field.Body, right)
			: Expression.LessThanOrEqual(field.Body, right);

		return Expression.Lambda<Func<T, bool>>(body, field.Parameters);
	}

	/// <summary>
	/// 从请求参数中取出时间值，兼容字符串形式的日期时间
	/// </summary>
	/// <param name="parameters">请求参数</param>
	/// <param name="key">参数名</param>
	/// <returns>解析成功返回时间值，否则返回 null</returns>
	private static DateTime? GetDateTime(IDictionary<string, object> parameters, string key)
	{
		if (parameters == null || !parameters.TryGetValue(key, out var raw) || raw == null)
		{
			return null;
		}

		if (raw is DateTime time)
		{
			return time;
		}

		return DateTime.TryParse(Convert.ToString(raw), out var parsed) ? parsed : null;
	}
}