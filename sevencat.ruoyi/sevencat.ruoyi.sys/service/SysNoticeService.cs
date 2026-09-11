using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.db;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 公告业务层（对应 Java 的 <c>ISysNoticeService</c> / <c>SysNoticeServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端 VO 上的 <c>@Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "createBy")</c>
/// 由翻译插件自动回填 <c>createByName</c>，C# 端无翻译注解，改为查询后按 <c>CreateBy</c> 手工回填。
/// </remarks>
[Component]
public class SysNoticeService(IFreeSql fsql, IMapper mapper,IIdGen idgen)
{
	/// <summary>
	/// 分页查询通知公告列表（对应 Java 的 <c>selectPageNoticeList</c>）
	/// </summary>
	/// <param name="notice">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>通知公告分页列表（已回填创建人名称）</returns>
	public async Task<PageResult<SysNoticeVo>> SelectPageNoticeList(SysNoticeBo notice, PageQuery2 pageQuery)
	{
		var page = await (await BuildNoticeQuery(notice)).ToPage(pageQuery);
		var result = page.MapTo<SysNoticeVo>(mapper);
		await FillCreateByName(result.Rows);
		return result;
	}

	/// <summary>
	/// 查询公告信息（对应 Java 的 <c>selectNoticeById</c>）
	/// </summary>
	/// <param name="noticeId">公告ID</param>
	/// <returns>公告信息（已回填创建人名称）；不存在时返回 null</returns>
	public async Task<SysNoticeVo> SelectNoticeById(long noticeId)
	{
		var row = await fsql.Select<TSysNotice>()
			.Where(x => x.NoticeId == noticeId)
			.FirstAsync();
		if (row == null)
		{
			return null;
		}

		var vo = row.MapTo<SysNoticeVo>(mapper);
		await FillCreateByName([vo]);
		return vo;
	}

	/// <summary>
	/// 查询公告列表（对应 Java 的 <c>selectNoticeList</c>）
	/// </summary>
	/// <param name="notice">公告信息</param>
	/// <returns>公告集合（已回填创建人名称）</returns>
	public async Task<List<SysNoticeVo>> SelectNoticeList(SysNoticeBo notice)
	{
		var rows = await (await BuildNoticeQuery(notice)).ToListAsync();
		var list = rows.MapTo<List<SysNoticeVo>>(mapper);
		await FillCreateByName(list);
		return list;
	}


	/// <summary>
	/// 新增公告（对应 Java 的 <c>insertNotice</c>）
	/// </summary>
	/// <param name="bo">公告信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> InsertNotice(SysNoticeBo bo)
	{
		var notice = bo.MapTo<TSysNotice>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		notice.CreateBy ??= await LoginHelper.GetLoginUid();
		notice.NoticeId = idgen.NextId();
		var rows = await fsql.Insert(notice).ExecuteAffrowsAsync();
		// 对应 Java 的 bo.setNoticeId(notice.getNoticeId())，供调用方接着广播公告
		bo.NoticeId = notice.NoticeId;
		return rows;
	}

	/// <summary>
	/// 修改公告（对应 Java 的 <c>updateNotice</c>）
	/// </summary>
	/// <param name="bo">公告信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateNotice(SysNoticeBo bo)
	{
		var notice = bo.MapTo<TSysNotice>(mapper);
		notice.UpdateBy ??= await LoginHelper.GetLoginUid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		return await fsql.Update<TSysNotice>()
			.SetSourceIgnore(notice)
			.Where(a => a.NoticeId == notice.NoticeId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 删除公告（对应 Java 的 <c>deleteNoticeById</c>）
	/// </summary>
	/// <param name="noticeId">公告ID</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteNoticeById(long noticeId)
	{
		return await fsql.Delete<TSysNotice>()
			.Where(x => x.NoticeId == noticeId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 批量删除公告信息（对应 Java 的 <c>deleteNoticeByIds</c>）
	/// </summary>
	/// <param name="noticeIds">需要删除的公告ID</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteNoticeByIds(List<long> noticeIds)
	{
		if (noticeIds == null || noticeIds.Count == 0)
		{
			return 0;
		}

		return await fsql.Delete<TSysNotice>()
			.Where(x => noticeIds.Contains(x.NoticeId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 构造公告列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">公告筛选条件</param>
	/// <returns>公告列表查询对象</returns>
	private async Task<ISelect<TSysNotice>> BuildNoticeQuery(SysNoticeBo bo)
	{
		var q = fsql.Select<TSysNotice>()
			.WhereLike(bo.NoticeTitle, x => x.NoticeTitle)
			.WhereHasTextEq(bo.NoticeType, x => x.NoticeType);

		if (bo.CreateByName.IsNotNullOrWhiteSpace())
		{
			// 对应 Java 的先按用户名查用户，再用 userId 过滤 createBy
			var userId = await fsql.Select<TSysUser>()
				.Where(x => x.UserName == bo.CreateByName)
				.FirstAsync(x => (long?)x.UserId);
			// 用户不存在时 Java 生成的 create_by = null 恒不成立，这里用 1 = 0 保持「查不到数据」的语义
			q = userId.HasValue
				? q.Where(x => x.CreateBy == userId.Value)
				: q.Where("1 = 0");
		}

		return q.OrderBy(x => x.NoticeId);
	}

	/// <summary>
	/// 回填公告列表的创建人名称（对应 Java 的 @Translation USER_ID_TO_NAME）
	/// </summary>
	/// <param name="rows">公告列表</param>
	private async Task FillCreateByName(List<SysNoticeVo> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		var userIds = rows.Where(x => x.CreateBy.HasValue)
			.Select(x => x.CreateBy.Value)
			.Distinct()
			.ToList();
		if (userIds.Count == 0)
		{
			return;
		}

		var users = await fsql.Select<TSysUser>()
			.Where(x => userIds.Contains(x.UserId))
			.ToListAsync(x => new { x.UserId, x.UserName });
		var userNames = users.ToDictionary(x => x.UserId, x => x.UserName);

		foreach (var row in rows)
		{
			if (row.CreateBy.HasValue && userNames.TryGetValue(row.CreateBy.Value, out var userName))
			{
				row.CreateByName = userName;
			}
		}
	}
}
