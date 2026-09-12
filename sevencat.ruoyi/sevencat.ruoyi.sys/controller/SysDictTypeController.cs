using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 数据字典信息（对应 Java 的 <c>SysDictTypeController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/dict/type")]
public class SysDictTypeController(
	SysDictTypeService dictTypeService,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 字典类型导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "字典类型";

	/// <summary>
	/// 分页查询字典类型列表。
	/// </summary>
	/// <param name="dictType">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>字典类型分页结果</returns>
	[SaCheckPermission("system:dict:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysDictTypeVo>>> List([FromQuery] SysDictTypeBo dictType,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await dictTypeService.SelectPageDictTypeList(dictType, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 导出字典类型列表。
	/// </summary>
	/// <param name="dictType">查询条件</param>
	[Log("字典类型", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:dict:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysDictTypeBo dictType)
	{
		var list = await dictTypeService.SelectDictTypeList(dictType);
		await ExcelHttpExt.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 查询字典类型详细
	/// </summary>
	/// <param name="dictId">字典ID</param>
	/// <returns>字典类型详情</returns>
	[SaCheckPermission("system:dict:query")]
	[HttpGet("{dictId:long}")]
	public async Task<CommonResult<SysDictTypeVo>> GetInfo([FromRoute] long dictId)
	{
		return (await dictTypeService.SelectDictTypeById(dictId)).ToCommonResult();
	}

	/// <summary>
	/// 新增字典类型。
	/// </summary>
	/// <param name="dict">字典类型参数</param>
	/// <returns>操作结果</returns>
	[Log("字典类型", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:dict:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysDictTypeBo dict)
	{
		if (!await dictTypeService.CheckDictTypeUnique(dict))
		{
			return CommonResult.Fail($"新增字典'{dict.DictName}'失败，字典类型已存在");
		}

		await dictTypeService.InsertDictType(dict);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 修改字典类型。
	/// </summary>
	/// <param name="dict">字典类型参数</param>
	/// <returns>操作结果</returns>
	[Log("字典类型", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:dict:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysDictTypeBo dict)
	{
		if (!await dictTypeService.CheckDictTypeUnique(dict))
		{
			return CommonResult.Fail($"修改字典'{dict.DictName}'失败，字典类型已存在");
		}

		await dictTypeService.UpdateDictType(dict);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 删除字典类型
	/// </summary>
	/// <param name="dictIds">字典ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("字典类型", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] dictIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:dict:remove")]
	[HttpDelete("{dictIds}")]
	public async Task<CommonResult> Remove([FromRoute] string dictIds)
	{
		await dictTypeService.DeleteDictTypeByIds(ParseLongList(dictIds));
		return CommonResult.Ok();
	}

	/// <summary>
	/// 刷新字典缓存。
	/// </summary>
	/// <returns>操作结果</returns>
	[Log("字典类型", BusinessTypeEnum.Clean)]
	// Java 原注解 @Lock4j：C# 端无分布式锁设施，未实现
	[SaCheckPermission("system:dict:remove")]
	[HttpDelete("refreshCache")]
	public async Task<CommonResult> RefreshCache()
	{
		await dictTypeService.ResetDictCache();
		return CommonResult.Ok();
	}

	/// <summary>
	/// 获取字典类型下拉选择列表。
	/// </summary>
	/// <returns>字典类型列表</returns>
	[HttpGet("optionselect")]
	public async Task<CommonResult<List<SysDictTypeVo>>> Optionselect()
	{
		return (await dictTypeService.SelectDictTypeAll()).ToCommonResult();
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(dictIds)</c>）
	/// </summary>
	/// <param name="value">逗号分隔的ID串</param>
	/// <returns>ID列表</returns>
	private static List<long> ParseLongList(string value)
	{
		if (value.IsNullOrWhiteSpace())
		{
			return [];
		}

		return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(item => long.TryParse(item, out var id) ? id : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id.Value)
			.ToList();
	}
}
