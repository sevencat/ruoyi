using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 操作日志业务层（对应 Java 的 <c>ISysOperLogService</c> / <c>SysOperLogServiceImpl</c>）
/// </summary>
[Component]
public class SysOperLogService(IFreeSql fsql, IMapper mapper)
{
	private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 分页查询操作日志列表（对应 Java 的 <c>queryPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>操作日志分页列表</returns>
	public async Task<PageResult<SysOperLogVo>> SelectPageOperLogList(SysOperLogBo bo, PageQuery2 pageQuery)
	{
		var page = await BuildOperLogQuery(bo).ToPage(pageQuery);
		return page.MapTo<SysOperLogVo>(mapper);
	}

	/// <summary>
	/// 查询操作日志列表（对应 Java 的 <c>queryList</c>，导出时使用）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>操作日志列表</returns>
	public async Task<List<SysOperLogVo>> SelectOperLogList(SysOperLogBo bo)
	{
		var rows = await BuildOperLogQuery(bo).ToListAsync();
		return rows.MapTo<List<SysOperLogVo>>(mapper);
	}

	/// <summary>
	/// 查询操作日志详细（对应 Java 的 <c>queryById</c>）
	/// </summary>
	/// <param name="operId">日志主键</param>
	/// <returns>操作日志详情；不存在时返回 null</returns>
	public async Task<SysOperLogVo> SelectOperLogById(long operId)
	{
		var row = await fsql.Select<TSysOperLog>()
			.Where(x => x.OperId == operId)
			.FirstAsync();
		return row?.MapTo<SysOperLogVo>(mapper);
	}

	/// <summary>
	/// 批量删除操作日志（对应 Java 的 <c>deleteOperLogByIds</c>）
	/// </summary>
	/// <param name="operIds">需要删除的日志主键</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteOperLogByIds(List<long> operIds)
	{
		if (operIds == null || operIds.Count == 0)
		{
			return 0;
		}

		return await fsql.Delete<TSysOperLog>()
			.Where(x => operIds.Contains(x.OperId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 清空操作日志（对应 Java 的 <c>cleanOperLog</c>）
	/// </summary>
	/// <returns>影响行数</returns>
	public async Task<int> CleanOperLog()
	{
		// 对应 Java 的 operLogMapper.delete(null)，即删除全表数据
		return await fsql.Delete<TSysOperLog>()
			.Where("1 = 1")
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 新增操作日志（对应 Java 的 <c>insertOperlog</c>，由 LogAspect 调用）
	/// </summary>
	/// <param name="bo">操作日志</param>
	/// <returns>影响行数，记录失败时返回 0</returns>
	/// <remarks>记录操作日志属于旁路行为，异常只记日志不外抛，避免影响业务接口本身。</remarks>
	public async Task<int> RecordOperLog(SysOperLogBo bo)
	{
		if (bo == null)
		{
			return 0;
		}

		var log = bo.MapTo<TSysOperLog>(mapper);
		log.OperTime ??= DateTime.Now;
		try
		{
			return await fsql.Insert(log).ExecuteAffrowsAsync();
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "记录操作日志失败:{0}", log.Title);
			return 0;
		}
	}

	/// <summary>
	/// 构造操作日志列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">筛选条件</param>
	/// <returns>操作日志查询对象</returns>
	private ISelect<TSysOperLog> BuildOperLogQuery(SysOperLogBo bo)
	{
		var q = fsql.Select<TSysOperLog>()
			.WhereLike(bo.Title, x => x.Title)
			.WhereNotNullEq(bo.BusinessType, x => x.BusinessType)
			.WhereHasTextEq(bo.OperName, x => x.OperName)
			.WhereNotNullEq(bo.Status, x => x.Status)
			.WhereTimeRange(bo.Params, x => x.OperTime)
			.OrderByDescending(x => x.OperId);

		// 对应 Java 的 in(businessType, Arrays.asList(bo.getBusinessTypes()))
		if (bo.BusinessTypes is { Length: > 0 })
		{
			var types = bo.BusinessTypes.Select(x => (int?)x).ToList();
			q = q.Where(x => types.Contains(x.BusinessType));
		}

		return q;
	}
}
