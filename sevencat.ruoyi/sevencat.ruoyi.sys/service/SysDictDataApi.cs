using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.service;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 字典数据业务层（对应 Java 的 <c>ISysDictDataService</c> / <c>SysDictDataServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端字典数据在启动时全量预热进 Redis 缓存（<c>CacheNames.SYS_DICT</c>），
/// C# 端暂无该缓存，改为按需查询数据库；相应地 <c>@CachePut</c> / <c>CacheUtils.evict</c> 均无需实现。
/// </remarks>
[Component]
public class SysDictDataApi(IFreeSql fsql, IMapper mapper)
	: ISysDictDataApi
{
	/// <summary>
	/// 查询字典值到字典标签的映射（导出方向转换用）
	/// </summary>
	/// <param name="dictType">字典类型，如 <c>sys_user_gender</c></param>
	/// <returns>字典值到字典标签的映射</returns>
	public async Task<Dictionary<string, string>> SelectDictLabelMap(string dictType)
	{
		var rows = await fsql.Select<TSysDictData>()
			.Where(x => x.DictType == dictType)
			.OrderBy(x => x.DictSort)
			.ToListAsync(x => new { x.DictValue, x.DictLabel });

		var map = new Dictionary<string, string>();
		foreach (var row in rows)
		{
			// 字典值唯一，重复时保留排序靠前的一条
			if (!map.ContainsKey(row.DictValue))
			{
				map[row.DictValue] = row.DictLabel;
			}
		}

		return map;
	}

	/// <summary>
	/// 查询字典标签到字典值的映射（导入方向转换用）
	/// </summary>
	/// <param name="dictType">字典类型，如 <c>sys_user_gender</c></param>
	/// <returns>字典标签到字典值的映射</returns>
	public async Task<Dictionary<string, string>> SelectDictValueMap(string dictType)
	{
		var rows = await fsql.Select<TSysDictData>()
			.Where(x => x.DictType == dictType)
			.OrderBy(x => x.DictSort)
			.ToListAsync(x => new { x.DictValue, x.DictLabel });

		var map = new Dictionary<string, string>();
		foreach (var row in rows)
		{
			// 字典标签可能重复，取排序靠前的一条
			if (!map.ContainsKey(row.DictLabel))
			{
				map[row.DictLabel] = row.DictValue;
			}
		}

		return map;
	}

	/// <summary>
	/// 查询字典标签（对应 Java 的 <c>DictService.getDictLabel</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <param name="dictValue">字典值，可用 <paramref name="separator"/> 分隔多个值</param>
	/// <param name="separator">多值分隔符</param>
	/// <returns>字典标签；无匹配时返回空字符串</returns>
	public async Task<string> SelectDictLabel(string dictType, string dictValue,
		string separator = ExcelDictConvert.SEPARATOR)
	{
		if (dictValue.IsNullOrWhiteSpace())
		{
			return string.Empty;
		}

		return ExcelDictConvert.ConvertByMap(dictValue, await SelectDictLabelMap(dictType), separator);
	}

	/// <summary>
	/// 查询字典值（对应 Java 的 <c>DictService.getDictValue</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <param name="dictLabel">字典标签，可用 <paramref name="separator"/> 分隔多个值</param>
	/// <param name="separator">多值分隔符</param>
	/// <returns>字典值；无匹配时返回空字符串</returns>
	public async Task<string> SelectDictValue(string dictType, string dictLabel,
		string separator = ExcelDictConvert.SEPARATOR)
	{
		if (dictLabel.IsNullOrWhiteSpace())
		{
			return string.Empty;
		}

		return ExcelDictConvert.ReverseByMap(dictLabel, await SelectDictLabelMap(dictType), separator);
	}

	/// <summary>
	/// 分页查询字典数据列表（对应 Java 的 <c>selectPageDictDataList</c>）
	/// </summary>
	/// <param name="dictData">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>字典数据分页列表</returns>
	public async Task<PageResult<SysDictDataVo>> SelectPageDictDataList(SysDictDataBo dictData, PageQuery2 pageQuery)
	{
		var page = await BuildDictDataQuery(dictData).ToPage(pageQuery);
		return page.MapTo<SysDictDataVo>(mapper);
	}

	/// <summary>
	/// 根据条件查询字典数据列表（对应 Java 的 <c>selectDictDataList</c>）
	/// </summary>
	/// <param name="dictData">查询条件</param>
	/// <returns>字典数据集合信息</returns>
	public async Task<List<SysDictDataVo>> SelectDictDataList(SysDictDataBo dictData)
	{
		var rows = await BuildDictDataQuery(dictData).ToListAsync();
		return rows.MapTo<List<SysDictDataVo>>(mapper);
	}

	/// <summary>
	/// 根据字典类型查询字典数据（对应 Java 的 <c>selectDictDataByType</c>）
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <returns>字典数据集合信息</returns>
	public async Task<List<SysDictDataVo>> SelectDictDataByType(string dictType)
	{
		var rows = await fsql.Select<TSysDictData>()
			.Where(x => x.DictType == dictType)
			.OrderBy(x => x.DictSort)
			.ToListAsync();

		return rows.MapTo<List<SysDictDataVo>>(mapper);
	}

	/// <summary>
	/// 根据字典数据ID查询信息（对应 Java 的 <c>selectDictDataById</c>）
	/// </summary>
	/// <param name="dictCode">字典数据ID</param>
	/// <returns>字典数据；不存在时返回 null</returns>
	public async Task<SysDictDataVo> SelectDictDataById(long dictCode)
	{
		var row = await fsql.Select<TSysDictData>()
			.Where(x => x.DictCode == dictCode)
			.FirstAsync();

		return row?.MapTo<SysDictDataVo>(mapper);
	}

	/// <summary>
	/// 批量删除字典数据信息（对应 Java 的 <c>deleteDictDataByIds</c>）
	/// </summary>
	/// <param name="dictCodes">需要删除的字典数据ID</param>
	public async Task DeleteDictDataByIds(List<long> dictCodes)
	{
		if (dictCodes == null || dictCodes.Count == 0)
		{
			return;
		}

		// Java 先按 ID 查出 dictType 再逐条清理 SYS_DICT 缓存，C# 端无字典缓存，直接删除
		await fsql.Delete<TSysDictData>()
			.Where(x => dictCodes.Contains(x.DictCode))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 新增保存字典数据信息（对应 Java 的 <c>insertDictData</c>）
	/// </summary>
	/// <param name="bo">字典数据信息</param>
	/// <returns>该字典类型下的全部字典数据</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_DICT, key = "#bo.dictType")：C# 端无字典缓存，未实现
	public async Task<List<SysDictDataVo>> InsertDictData(SysDictDataBo bo)
	{
		var data = bo.MapTo<TSysDictData>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		data.CreateBy ??= await LoginHelper.GetLoginUid();

		var row = await fsql.Insert(data).ExecuteAffrowsAsync();
		if (row > 0)
		{
			return await SelectDictDataByType(data.DictType);
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 修改保存字典数据信息（对应 Java 的 <c>updateDictData</c>）
	/// </summary>
	/// <param name="bo">字典数据信息</param>
	/// <returns>该字典类型下的全部字典数据</returns>
	// Java 原注解 @CachePut(cacheNames = CacheNames.SYS_DICT, key = "#bo.dictType")：C# 端无字典缓存，未实现
	public async Task<List<SysDictDataVo>> UpdateDictData(SysDictDataBo bo)
	{
		var data = bo.MapTo<TSysDictData>(mapper);
		data.UpdateBy ??= await LoginHelper.GetLoginUid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		var row = await fsql.Update<TSysDictData>()
			.SetSourceIgnore(data)
			.Where(a => a.DictCode == data.DictCode)
			.ExecuteAffrowsAsync();
		if (row > 0)
		{
			return await SelectDictDataByType(data.DictType);
		}

		throw new ServiceException("操作失败");
	}

	/// <summary>
	/// 校验字典键值是否唯一（对应 Java 的 <c>checkDictDataUnique</c>）
	/// </summary>
	/// <param name="dict">字典数据</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckDictDataUnique(SysDictDataBo dict)
	{
		var dictCode = dict.DictCode;
		var exist = await fsql.Select<TSysDictData>()
			.Where(x => x.DictType == dict.DictType)
			.Where(x => x.DictValue == dict.DictValue)
			.WhereIf(dictCode.HasValue, x => x.DictCode != dictCode.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 构造字典数据列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">字典数据筛选条件</param>
	/// <returns>字典数据列表查询对象</returns>
	private ISelect<TSysDictData> BuildDictDataQuery(SysDictDataBo bo)
	{
		return fsql.Select<TSysDictData>()
			.WhereNotNullEq(bo.DictSort, x => x.DictSort)
			.WhereLike(bo.DictLabel, x => x.DictLabel)
			.WhereHasTextEq(bo.DictType, x => x.DictType)
			.OrderBy(x => x.DictSort)
			.OrderBy(x => x.DictCode);
	}
}