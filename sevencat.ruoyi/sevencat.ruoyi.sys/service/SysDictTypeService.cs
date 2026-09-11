using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 字典类型业务层（对应 Java 的 <c>ISysDictTypeService</c> / <c>SysDictTypeServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端字典数据与字典类型都缓存在 Redis（<c>CacheNames.SYS_DICT</c> / <c>CacheNames.SYS_DICT_TYPE</c>），
/// C# 端暂无该缓存，改为按需查询数据库，因此 <c>@Cacheable</c> / <c>@CachePut</c> 与 <c>resetDictCache</c> 均无需实现。
/// </remarks>
[Component]
public class SysDictTypeService(IFreeSql fsql, IMapper mapper, SysDictDataApi dictDataService,
	LoginService loginService)
{
	/// <summary>
	/// 分页查询字典类型列表（对应 Java 的 <c>selectPageDictTypeList</c>）
	/// </summary>
	/// <param name="dictType">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>字典类型分页列表</returns>
	public async Task<PageResult<SysDictTypeVo>> SelectPageDictTypeList(SysDictTypeBo dictType, PageQuery2 pageQuery)
	{
		var page = await BuildDictTypeQuery(dictType).ToPage(pageQuery);
		return page.MapTo<SysDictTypeVo>(mapper);
	}

	/// <summary>
	/// 根据条件查询字典类型列表（对应 Java 的 <c>selectDictTypeList</c>）
	/// </summary>
	/// <param name="dictType">查询条件</param>
	/// <returns>字典类型集合信息</returns>
	public async Task<List<SysDictTypeVo>> SelectDictTypeList(SysDictTypeBo dictType)
	{
		var rows = await BuildDictTypeQuery(dictType).ToListAsync();
		return rows.MapTo<List<SysDictTypeVo>>(mapper);
	}

	/// <summary>
	/// 查询所有字典类型（对应 Java 的 <c>selectDictTypeAll</c>）
	/// </summary>
	/// <returns>字典类型集合信息</returns>
	public async Task<List<SysDictTypeVo>> SelectDictTypeAll()
	{
		// Java 未显式排序，这里按主键排序，保证下拉列表顺序稳定
		var rows = await fsql.Select<TSysDictType>()
			.OrderBy(x => x.DictId)
			.ToListAsync();

		return rows.MapTo<List<SysDictTypeVo>>(mapper);
	}

	/// <summary>
	/// 根据字典类型查询字典数据（对应 Java 的 <c>selectDictDataByType</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <returns>字典数据集合信息</returns>
	public Task<List<SysDictDataVo>> SelectDictDataByType(string dictType)
	{
		return dictDataService.SelectDictDataByType(dictType);
	}

	/// <summary>
	/// 根据字典类型ID查询信息（对应 Java 的 <c>selectDictTypeById</c>）
	/// </summary>
	/// <param name="dictId">字典类型ID</param>
	/// <returns>字典类型；不存在时返回 null</returns>
	public async Task<SysDictTypeVo> SelectDictTypeById(long dictId)
	{
		var row = await fsql.Select<TSysDictType>()
			.Where(x => x.DictId == dictId)
			.FirstAsync();

		return row?.MapTo<SysDictTypeVo>(mapper);
	}

	/// <summary>
	/// 根据字典类型查询信息（对应 Java 的 <c>selectDictTypeByType</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <returns>字典类型；不存在时返回 null</returns>
	public async Task<SysDictTypeVo> SelectDictTypeByType(string dictType)
	{
		var row = await fsql.Select<TSysDictType>()
			.Where(x => x.DictType == dictType)
			.FirstAsync();

		return row?.MapTo<SysDictTypeVo>(mapper);
	}

	/// <summary>
	/// 批量删除字典类型信息（对应 Java 的 <c>deleteDictTypeByIds</c>）
	/// </summary>
	/// <param name="dictIds">需要删除的字典ID</param>
	public async Task DeleteDictTypeByIds(List<long> dictIds)
	{
		if (dictIds == null || dictIds.Count == 0)
		{
			return;
		}

		var list = await fsql.Select<TSysDictType>()
			.Where(x => dictIds.Contains(x.DictId))
			.ToListAsync();

		// 已被字典数据引用的类型不允许删除
		foreach (var item in list)
		{
			if (await fsql.Select<TSysDictData>().Where(x => x.DictType == item.DictType).AnyAsync())
			{
				throw new ServiceException($"{item.DictName}已分配,不能删除");
			}
		}

		await fsql.Delete<TSysDictType>()
			.Where(x => dictIds.Contains(x.DictId))
			.ExecuteAffrowsAsync();

		// Java 此处还会失效 SYS_DICT / SYS_DICT_TYPE 缓存，C# 端无字典缓存，无需处理
	}

	/// <summary>
	/// 重置字典缓存数据（对应 Java 的 <c>resetDictCache</c>）
	/// </summary>
	public Task ResetDictCache()
	{
		// C# 端无字典缓存，无缓存可清理
		return Task.CompletedTask;
	}

	/// <summary>
	/// 新增保存字典类型信息（对应 Java 的 <c>insertDictType</c>）
	/// </summary>
	/// <param name="bo">字典类型信息</param>
	/// <returns>该字典类型下的全部字典数据，新增时恒为空列表</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_DICT, key = "#bo.dictType")：C# 端无字典缓存，未实现
	public async Task<List<SysDictDataVo>> InsertDictType(SysDictTypeBo bo)
	{
		var dict = bo.MapTo<TSysDictType>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		dict.CreateBy ??= await loginService.GetLoginuid();

		var row = await fsql.Insert(dict).ExecuteAffrowsAsync();
		if (row > 0)
		{
			// 新增 type 下无 data 数据，返回空列表防止缓存穿透（对应 Java 的 return new ArrayList<>()）
			return [];
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 修改保存字典类型信息（对应 Java 的 <c>updateDictType</c>）
	/// </summary>
	/// <param name="bo">字典类型信息</param>
	/// <returns>变更后该字典类型下的全部字典数据</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_DICT, key = "#bo.dictType")：C# 端无字典缓存，未实现
	public async Task<List<SysDictDataVo>> UpdateDictType(SysDictTypeBo bo)
	{
		var dict = bo.MapTo<TSysDictType>(mapper);
		dict.UpdateBy ??= await loginService.GetLoginuid();

		// 字典类型被改动时，同步刷新字典数据上引用的类型（对应 Java 的 sys_dict_data 批量 update）
		var oldDict = await fsql.Select<TSysDictType>()
			.Where(x => x.DictId == dict.DictId)
			.FirstAsync();

		await fsql.Update<TSysDictData>()
			.Set(a => a.DictType, dict.DictType)
			.Where(a => a.DictType == oldDict.DictType)
			.ExecuteAffrowsAsync();

		var row = await fsql.Update<TSysDictType>()
			.SetSourceIgnore(dict)
			.Where(a => a.DictId == dict.DictId)
			.ExecuteAffrowsAsync();
		if (row > 0)
		{
			return await dictDataService.SelectDictDataByType(dict.DictType);
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 校验字典类型是否唯一（对应 Java 的 <c>checkDictTypeUnique</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckDictTypeUnique(SysDictTypeBo dictType)
	{
		var dictId = dictType.DictId;
		var exist = await fsql.Select<TSysDictType>()
			.Where(x => x.DictType == dictType.DictType)
			.WhereIf(dictId.HasValue, x => x.DictId != dictId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 构造字典类型列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">字典类型筛选条件</param>
	/// <returns>字典类型列表查询对象</returns>
	private ISelect<TSysDictType> BuildDictTypeQuery(SysDictTypeBo bo)
	{
		return fsql.Select<TSysDictType>()
			.WhereLike(bo.DictName, x => x.DictName)
			.WhereLike(bo.DictType, x => x.DictType)
			.OrderBy(x => x.DictId);
	}
}
