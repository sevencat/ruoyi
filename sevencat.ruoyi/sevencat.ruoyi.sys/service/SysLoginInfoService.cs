using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.util;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 系统访问记录业务层（对应 Java 的 <c>ISysLoginInfoService</c> / <c>SysLoginInfoServiceImpl</c>）
/// </summary>
[Component]
public class SysLoginInfoService(IFreeSql fsql, IMapper mapper, IHttpContextAccessor httpCtxAccessor)
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 分页查询系统访问记录列表（对应 Java 的 <c>queryPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>访问记录分页列表</returns>
	public async Task<PageResult<SysLoginInfoVo>> SelectPageLoginInfoList(SysLoginInfoBo bo, PageQuery2 pageQuery)
	{
		var page = await BuildLoginInfoQuery(bo).ToPage(pageQuery);
		return page.MapTo<SysLoginInfoVo>(mapper);
	}

	/// <summary>
	/// 查询系统访问记录列表（对应 Java 的 <c>queryList</c>，导出时使用）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>访问记录列表</returns>
	public async Task<List<SysLoginInfoVo>> SelectLoginInfoList(SysLoginInfoBo bo)
	{
		var rows = await BuildLoginInfoQuery(bo).ToListAsync();
		return rows.MapTo<List<SysLoginInfoVo>>(mapper);
	}

	/// <summary>
	/// 批量删除系统访问记录（对应 Java 的 <c>deleteLoginInfoByIds</c>）
	/// </summary>
	/// <param name="infoIds">需要删除的访问ID</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteLoginInfoByIds(List<long> infoIds)
	{
		if (infoIds == null || infoIds.Count == 0)
		{
			return 0;
		}

		return await fsql.Delete<TSysLoginInfo>()
			.Where(x => infoIds.Contains(x.InfoId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 清空系统访问记录（对应 Java 的 <c>cleanLoginInfo</c>）
	/// </summary>
	/// <returns>影响行数</returns>
	public async Task<int> CleanLoginInfo()
	{
		// 对应 Java 的 loginInfoMapper.delete(null)，即删除全表数据
		return await fsql.Delete<TSysLoginInfo>()
			.Where("1 = 1")
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 记录登录日志（对应 Java 的 <c>AsyncFactory.recordLogininfor</c> 与
	/// <c>SysLoginInfoServiceImpl.recordLoginInfo</c>）。
	/// </summary>
	/// <param name="userName">登录账号</param>
	/// <param name="clientId">客户端id（对应 sys_client.client_id）</param>
	/// <param name="status">
	/// 登录状态，<see cref="SystemConstants.NORMAL"/> 表示成功、<see cref="SystemConstants.DISABLE"/> 表示失败
	/// </param>
	/// <param name="msg">提示消息</param>
	/// <param name="loginUser">
	/// 登录成功时的登录用户；登录失败时尚无登录用户，传 null，此时终端信息直接从请求上下文取
	/// </param>
	/// <returns>影响行数，记录失败时返回 0</returns>
	/// <remarks>
	/// Java 端经 AsyncManager 异步落库，C# 端同步落库并吞掉异常：记录登录日志属于旁路行为，不应影响登录主流程。
	/// </remarks>
	public async Task<int> RecordLoginInfo(string userName, string clientId, string status, string msg,
		LoginUser loginUser = null)
	{
		var info = new TSysLoginInfo
		{
			UserName = loginUser?.Username ?? userName,
			ClientKey = loginUser?.ClientKey ?? clientId,
			// Java 端取自 sys_client.device_type，SysClient 尚未实现前固定为 pc
			DeviceType = loginUser?.DeviceType ?? DeviceType.PC,
			Ipaddr = loginUser?.Ipaddr,
			LoginLocation = loginUser?.LoginLocation,
			Browser = loginUser?.Browser,
			Os = loginUser?.Os,
			Status = status,
			Msg = msg,
			LoginTime = DateTime.Now,
		};

		if (loginUser == null)
		{
			FillTerminalInfo(info);
		}

		try
		{
			return await fsql.Insert(info).ExecuteAffrowsAsync();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "记录登录日志失败:{0}", info.UserName);
			return 0;
		}
	}

	/// <summary>
	/// 从当前请求上下文补充终端信息（IP、登录地点、浏览器、操作系统）。
	/// </summary>
	/// <param name="info">登录日志</param>
	private void FillTerminalInfo(TSysLoginInfo info)
	{
		var httpctx = httpCtxAccessor.HttpContext;
		if (httpctx == null)
		{
			return;
		}

		info.Ipaddr = ServletUtils.GetClientIp(httpctx);
		if (info.Ipaddr.IsNotNullOrWhiteSpace())
		{
			info.LoginLocation = AddressUtils.GetRealAddressByIP(info.Ipaddr);
		}

		var (browser, os) = UserAgentUtils.Parse(ServletUtils.GetUserAgent(httpctx));
		info.Browser = browser;
		info.Os = os;
	}

	/// <summary>
	/// 构造访问记录列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">筛选条件</param>
	/// <returns>访问记录查询对象</returns>
	private ISelect<TSysLoginInfo> BuildLoginInfoQuery(SysLoginInfoBo bo)
	{
		return fsql.Select<TSysLoginInfo>()
			.WhereLike(bo.UserName, x => x.UserName)
			.WhereHasTextEq(bo.Status, x => x.Status)
			.WhereLike(bo.Ipaddr, x => x.Ipaddr)
			.WhereTimeRange(bo.Params, x => x.LoginTime)
			.OrderByDescending(x => x.InfoId);
	}
}
