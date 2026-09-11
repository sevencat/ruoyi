using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 客户端管理业务层（对应 Java 的 <c>ISysClientService</c> / <c>SysClientServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端客户端信息缓存在 Redis（<c>CacheNames.SYS_CLIENT</c>），C# 端暂无该缓存，改为每次都查库，
/// 因此 <c>@Cacheable</c> / <c>@CacheEvict</c> 均无需实现。
/// Java 中 <c>queryByClientId</c> 属于认证模块调用入口，本项目暂无调用方，但为保持接口完整仍予以实现。
/// </remarks>
[Component]
public class SysClientService(IFreeSql fsql, IMapper mapper)
{
	/// <summary>
	/// 扩展规则串的分隔符（对应 Java 的 <c>[,\;\r\n]+</c>）
	/// </summary>
	private static readonly Regex RuleSeparatorRegex = new("[,;\\r\\n]+", RegexOptions.Compiled);

	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const string DEL_FLAG_DELETED = "1";

	/// <summary>
	/// 组合串使用的分隔符（对应 Java 的 <c>StringUtils.SEPARATOR</c>）
	/// </summary>
	private const string SEPARATOR = ",";

	/// <summary>
	/// 查询客户端详情（对应 Java 的 <c>queryById</c>）
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>客户端详情；不存在时返回 null</returns>
	public async Task<SysClientVo> QueryById(long id)
	{
		var row = await fsql.Select<TSysClient>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.Id == id)
			.FirstAsync();

		var vo = row?.MapTo<SysClientVo>(mapper);
		FillClientRuleFields(vo);
		return vo;
	}

	/// <summary>
	/// 根据客户端标识查询客户端详情（对应 Java 的 <c>queryByClientId</c>）
	/// </summary>
	/// <param name="clientId">客户端标识</param>
	/// <returns>客户端详情；不存在时返回 null</returns>
	public async Task<SysClientVo> QueryByClientId(string clientId)
	{
		var row = await fsql.Select<TSysClient>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.ClientId == clientId)
			.FirstAsync();

		var vo = row?.MapTo<SysClientVo>(mapper);
		FillClientRuleFields(vo);
		return vo;
	}

	/// <summary>
	/// 分页查询客户端列表（对应 Java 的 <c>queryPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>客户端分页列表</returns>
	public async Task<PageResult<SysClientVo>> SelectPageClientList(SysClientBo bo, PageQuery2 pageQuery)
	{
		var page = await BuildClientQuery(bo).ToPage(pageQuery);
		var result = page.MapTo<SysClientVo>(mapper);
		result.Rows.ForEach(FillClientRuleFields);
		return result;
	}

	/// <summary>
	/// 查询客户端列表（对应 Java 的 <c>queryList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>客户端列表</returns>
	public async Task<List<SysClientVo>> SelectClientList(SysClientBo bo)
	{
		var rows = await BuildClientQuery(bo).ToListAsync();
		var list = rows.MapTo<List<SysClientVo>>(mapper);
		list.ForEach(FillClientRuleFields);
		return list;
	}

	/// <summary>
	/// 新增客户端（对应 Java 的 <c>insertByBo</c>）
	/// </summary>
	/// <param name="bo">客户端信息</param>
	/// <returns>新增成功返回 true</returns>
	public async Task<bool> InsertByBo(SysClientBo bo)
	{
		var add = bo.MapTo<TSysClient>(mapper);
		add.GrantType = bo.GrantTypeList == null ? null : string.Join(SEPARATOR, bo.GrantTypeList);
		add.AccessPath = ResolveRuleValue(bo.AccessPath, bo.AccessPathList, NormalizeAccessPath);
		add.IpWhitelist = ResolveRuleValue(bo.IpWhitelist, bo.IpWhitelistList, p => p);
		// 生成客户端标识（对应 Java 的 SecureUtil.md5(clientKey + clientSecret)）
		add.ClientId = Md5((bo.ClientKey ?? string.Empty) + (bo.ClientSecret ?? string.Empty));
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		add.CreateBy ??= await LoginHelper.GetLoginUid();

		var flag = await fsql.Insert(add).ExecuteAffrowsAsync() > 0;
		if (flag)
		{
			bo.Id = add.Id;
		}

		return flag;
	}

	/// <summary>
	/// 修改客户端（对应 Java 的 <c>updateByBo</c>）
	/// </summary>
	/// <param name="bo">客户端信息</param>
	/// <returns>修改成功返回 true</returns>
	public async Task<bool> UpdateByBo(SysClientBo bo)
	{
		var update = bo.MapTo<TSysClient>(mapper);
		update.GrantType = bo.GrantTypeList == null ? null : string.Join(SEPARATOR, bo.GrantTypeList);
		update.AccessPath = ResolveRuleValue(bo.AccessPath, bo.AccessPathList, NormalizeAccessPath);
		update.IpWhitelist = ResolveRuleValue(bo.IpWhitelist, bo.IpWhitelistList, p => p);
		update.UpdateBy ??= await LoginHelper.GetLoginUid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		return await fsql.Update<TSysClient>()
			.SetSourceIgnore(update)
			.Where(a => a.Id == update.Id)
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 修改客户端启停状态（对应 Java 的 <c>updateClientStatus</c>）
	/// </summary>
	/// <param name="clientId">客户端标识</param>
	/// <param name="status">状态值</param>
	/// <returns>更新条数</returns>
	public async Task<int> UpdateClientStatus(string clientId, string status)
	{
		return await fsql.Update<TSysClient>()
			.Set(a => a.Status, status)
			.Where(a => a.ClientId == clientId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 批量删除客户端（对应 Java 的 <c>deleteWithValidByIds</c>）
	/// </summary>
	/// <param name="ids">主键集合</param>
	/// <param name="isValid">是否执行业务校验（Java 端当前未做额外校验，保留入参以对齐接口）</param>
	/// <returns>删除成功返回 true</returns>
	public async Task<bool> DeleteWithValidByIds(List<long> ids, bool isValid)
	{
		if (ids == null || ids.Count == 0)
		{
			return false;
		}

		// 对应 Java 的 @TableLogic 逻辑删除：deleteByIds 实际是 update del_flag = '1'
		return await fsql.Update<TSysClient>()
			.Set(a => a.DelFlag, DEL_FLAG_DELETED)
			.Where(a => ids.Contains(a.Id))
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 校验客户端key是否唯一（对应 Java 的 <c>checkClickKeyUnique</c>）
	/// </summary>
	/// <param name="bo">客户端信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckClientKeyUnique(SysClientBo bo)
	{
		var exist = await fsql.Select<TSysClient>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.ClientKey == bo.ClientKey)
			.WhereIf(bo.Id.HasValue, x => x.Id != bo.Id.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 构造客户端列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">客户端筛选条件</param>
	/// <returns>客户端列表查询对象</returns>
	private ISelect<TSysClient> BuildClientQuery(SysClientBo bo)
	{
		return fsql.Select<TSysClient>()
			// 对应 Java 实体字段上的 @TableLogic，MyBatis-Plus 会隐式追加 del_flag = '0'
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereHasTextEq(bo.ClientId, x => x.ClientId)
			.WhereHasTextEq(bo.ClientKey, x => x.ClientKey)
			.WhereHasTextEq(bo.ClientSecret, x => x.ClientSecret)
			.WhereHasTextEq(bo.Status, x => x.Status)
			.OrderBy(x => x.Id);
	}

	/// <summary>
	/// 回填客户端扩展规则字段，便于前端直接展示和编辑（对应 Java 的 <c>fillClientRuleFields</c>）
	/// </summary>
	/// <param name="vo">客户端视图对象</param>
	private static void FillClientRuleFields(SysClientVo vo)
	{
		if (vo == null)
		{
			return;
		}

		vo.GrantTypeList = SplitRules(vo.GrantType);
		vo.AccessPathList = ParseRuleList(vo.AccessPath, NormalizeAccessPath);
		vo.IpWhitelistList = ParseRuleList(vo.IpWhitelist, p => p);
	}

	/// <summary>
	/// 统一处理白名单与路径规则的入库格式（对应 Java 的 <c>resolveRuleValue</c>）
	/// </summary>
	/// <param name="rawValue">原始字符串</param>
	/// <param name="listValue">列表值</param>
	/// <param name="normalizer">单条规则归一化器</param>
	/// <returns>逗号拼接后的规则串</returns>
	private static string ResolveRuleValue(string rawValue, List<string> listValue, Func<string, string> normalizer)
	{
		var rules = rawValue != null ? SplitRules(rawValue) : listValue;
		if (rules == null || rules.Count == 0)
		{
			// 对应 Java：listValue != null || rawValue != null ? "" : null
			return listValue != null || rawValue != null ? string.Empty : null;
		}

		return string.Join(SEPARATOR,
			rules.Select(normalizer).Where(p => p.IsNotNullOrWhiteSpace()));
	}

	/// <summary>
	/// 将规则串转换为列表（对应 Java 的 <c>parseRuleList</c>）
	/// </summary>
	/// <param name="value">规则串</param>
	/// <param name="normalizer">单条规则归一化器</param>
	/// <returns>规则列表</returns>
	private static List<string> ParseRuleList(string value, Func<string, string> normalizer)
	{
		return SplitRules(value).Select(normalizer).Where(p => p.IsNotNullOrWhiteSpace()).ToList();
	}

	/// <summary>
	/// 按分隔符切分规则串并去除空白项（对应 Java 的 <c>StringUtils.str2List(value, regex, true, true)</c>）
	/// </summary>
	/// <param name="value">规则串</param>
	/// <returns>规则列表</returns>
	private static List<string> SplitRules(string value)
	{
		if (value == null)
		{
			return [];
		}

		return RuleSeparatorRegex.Split(value)
			.Select(p => p.Trim())
			.Where(p => p.Length > 0)
			.ToList();
	}

	/// <summary>
	/// 统一补齐路径前导斜杠，避免配置成 app/** 时无法命中（对应 Java 的 <c>normalizeAccessPath</c>）
	/// </summary>
	/// <param name="path">路径规则</param>
	/// <returns>规范化后的路径规则</returns>
	private static string NormalizeAccessPath(string path)
	{
		if (path.IsNullOrWhiteSpace())
		{
			return null;
		}

		var accessPath = path.Trim();
		if (accessPath.Length == 0)
		{
			return null;
		}

		if (accessPath == "*" || accessPath == "/**")
		{
			return "/**";
		}

		return accessPath.StartsWith('/') ? accessPath : "/" + accessPath;
	}

	/// <summary>
	/// 计算 MD5 小写十六进制串（对应 Java 的 <c>SecureUtil.md5</c>）
	/// </summary>
	/// <param name="value">原文</param>
	/// <returns>32 位小写十六进制串</returns>
	private static string Md5(string value)
	{
		var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
		return Convert.ToHexString(bytes).ToLowerInvariant();
	}
}
