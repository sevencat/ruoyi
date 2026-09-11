using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 参数配置业务层（对应 Java 的 <c>ISysConfigService</c> / <c>SysConfigServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端参数配置缓存在 Redis（<c>CacheNames.SYS_CONFIG</c>），C# 端暂无该缓存，改为每次都查库，
/// 因此 <c>@Cacheable</c> / <c>@CachePut</c> / <c>CacheUtils.evict</c> 均无需实现，
/// <c>resetConfigCache</c> 也退化为空实现。
/// </remarks>
[Component]
public class SysConfigService(IFreeSql fsql, IMapper mapper, LoginService loginService)
{
	/// <summary>
	/// 分页查询参数配置列表（对应 Java 的 <c>selectPageConfigList</c>）
	/// </summary>
	/// <param name="config">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>参数配置分页列表</returns>
	public async Task<PageResult<SysConfigVo>> SelectPageConfigList(SysConfigBo config, PageQuery2 pageQuery)
	{
		var page = await BuildConfigQuery(config).ToPage(pageQuery);
		return page.MapTo<SysConfigVo>(mapper);
	}

	/// <summary>
	/// 查询参数配置列表（对应 Java 的 <c>selectConfigList</c>）
	/// </summary>
	/// <param name="config">参数配置信息</param>
	/// <returns>参数配置集合</returns>
	public async Task<List<SysConfigVo>> SelectConfigList(SysConfigBo config)
	{
		var rows = await BuildConfigQuery(config).ToListAsync();
		return rows.MapTo<List<SysConfigVo>>(mapper);
	}

	/// <summary>
	/// 查询参数配置信息（对应 Java 的 <c>selectConfigById</c>）
	/// </summary>
	/// <param name="configId">参数配置ID</param>
	/// <returns>参数配置信息；不存在时返回 null</returns>
	public async Task<SysConfigVo> SelectConfigById(long configId)
	{
		var row = await fsql.Select<TSysConfig>()
			.Where(x => x.ConfigId == configId)
			.FirstAsync();

		return row?.MapTo<SysConfigVo>(mapper);
	}

	/// <summary>
	/// 根据键名查询参数配置信息（对应 Java 的 <c>selectConfigByKey</c>）
	/// </summary>
	/// <param name="configKey">参数key</param>
	/// <returns>参数键值；不存在时返回空字符串</returns>
	// Java 原注解 @Cacheable(cacheNames = CacheNames.SYS_CONFIG, key = "#configKey")：C# 端无参数缓存，未实现
	public async Task<string> SelectConfigByKey(string configKey)
	{
		var value = await fsql.Select<TSysConfig>()
			.Where(x => x.ConfigKey == configKey)
			.FirstAsync(x => x.ConfigValue);

		// 对应 Java 的 ObjectUtils.notNullGetter(retConfig, SysConfig::getConfigValue, StringUtils.EMPTY)
		return value ?? string.Empty;
	}

	/// <summary>
	/// 新增参数配置（对应 Java 的 <c>insertConfig</c>）
	/// </summary>
	/// <param name="bo">参数配置信息</param>
	/// <returns>参数键值</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_CONFIG, key = "#bo.configKey")：C# 端无参数缓存，未实现
	public async Task<string> InsertConfig(SysConfigBo bo)
	{
		var config = bo.MapTo<TSysConfig>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		config.CreateBy ??= await loginService.GetLoginuid();

		var row = await fsql.Insert(config).ExecuteAffrowsAsync();
		if (row > 0)
		{
			return config.ConfigValue;
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 修改参数配置（对应 Java 的 <c>updateConfig</c>）
	/// </summary>
	/// <param name="bo">参数配置信息</param>
	/// <returns>参数键值</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_CONFIG, key = "#bo.configKey")：C# 端无参数缓存，未实现
	public async Task<string> UpdateConfig(SysConfigBo bo)
	{
		var config = bo.MapTo<TSysConfig>(mapper);
		config.UpdateBy ??= await loginService.GetLoginuid();

		int row;
		if (bo.ConfigId.HasValue)
		{
			// 传递了主键时按主键更新（对应 Java 的 updateById(config)）
			// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
			row = await fsql.Update<TSysConfig>()
				.SetSourceIgnore(config)
				.Where(a => a.ConfigId == config.ConfigId)
				.ExecuteAffrowsAsync();
		}
		else
		{
			// 未传主键时按键名更新（对应 Java 的 lambda().eq(SysConfig::getConfigKey, ...).updateCount(config)）
			row = await fsql.Update<TSysConfig>()
				.SetSourceIgnore(config)
				.Where(a => a.ConfigKey == config.ConfigKey)
				.ExecuteAffrowsAsync();
		}

		if (row > 0)
		{
			return config.ConfigValue;
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 批量删除参数信息（对应 Java 的 <c>deleteConfigByIds</c>）
	/// </summary>
	/// <param name="configIds">需要删除的参数ID</param>
	public async Task DeleteConfigByIds(List<long> configIds)
	{
		if (configIds == null || configIds.Count == 0)
		{
			return;
		}

		var list = await fsql.Select<TSysConfig>()
			.Where(x => configIds.Contains(x.ConfigId))
			.ToListAsync();

		// 内置参数不允许删除
		foreach (var config in list)
		{
			if (SystemConstants.YES == config.ConfigType)
			{
				throw new ServiceException($"内置参数【{config.ConfigKey}】不能删除");
			}
		}

		// Java 此处还会逐条 evict SYS_CONFIG 缓存，C# 端无参数缓存，直接删除
		await fsql.Delete<TSysConfig>()
			.Where(x => configIds.Contains(x.ConfigId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 重置参数缓存数据（对应 Java 的 <c>resetConfigCache</c>）
	/// </summary>
	public Task ResetConfigCache()
	{
		// C# 端无参数缓存，无缓存可清理
		return Task.CompletedTask;
	}

	/// <summary>
	/// 校验参数键名是否唯一（对应 Java 的 <c>checkConfigKeyUnique</c>）
	/// </summary>
	/// <param name="config">参数配置信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckConfigKeyUnique(SysConfigBo config)
	{
		var configId = config.ConfigId;
		var exist = await fsql.Select<TSysConfig>()
			.Where(x => x.ConfigKey == config.ConfigKey)
			.WhereIf(configId.HasValue, x => x.ConfigId != configId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 构造参数配置列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">参数配置筛选条件</param>
	/// <returns>参数配置列表查询对象</returns>
	private ISelect<TSysConfig> BuildConfigQuery(SysConfigBo bo)
	{
		return fsql.Select<TSysConfig>()
			.WhereLike(bo.ConfigName, x => x.ConfigName)
			.WhereHasTextEq(bo.ConfigType, x => x.ConfigType)
			.WhereLike(bo.ConfigKey, x => x.ConfigKey)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(bo.Params, x => x.CreateTime)
			.OrderBy(x => x.ConfigId);
	}
}
