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
/// 数据字典信息（对应 Java 的 <c>SysDictDataController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/dict/data")]
public class SysDictDataController(
	SysDictDataApi dictDataService,
	SysDictTypeService dictTypeService,
	ExcelDictFormatConverter dictFormatConverter,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 字典数据导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "字典数据";

	/// <summary>
	/// 分页查询字典数据列表。
	/// </summary>
	/// <param name="dictData">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>字典数据分页结果</returns>
	[SaCheckPermission("system:dict:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysDictDataVo>>> List([FromQuery] SysDictDataBo dictData,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await dictDataService.SelectPageDictDataList(dictData, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 导出字典数据列表。
	/// </summary>
	/// <param name="dictData">查询条件</param>
	[Log("字典数据", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:dict:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysDictDataBo dictData)
	{
		var list = await dictDataService.SelectDictDataList(dictData);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat / ExcelDictConvert 的转换器（如 isDefault 的 sys_yes_no）
		await dictFormatConverter.ToExcelData(list);
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 查询字典数据详细
	/// </summary>
	/// <param name="dictCode">字典code</param>
	/// <returns>字典数据详情</returns>
	[SaCheckPermission("system:dict:query")]
	[HttpGet("{dictCode:long}")]
	public async Task<CommonResult<SysDictDataVo>> GetInfo([FromRoute] long dictCode)
	{
		return (await dictDataService.SelectDictDataById(dictCode)).ToCommonResult();
	}

	/// <summary>
	/// 根据字典类型查询字典数据信息
	/// </summary>
	/// <param name="dictType">字典类型</param>
	/// <returns>字典数据列表</returns>
	[HttpGet("type/{dictType}")]
	public async Task<CommonResult<List<SysDictDataVo>>> DictType([FromRoute] string dictType)
	{
		var data = await dictTypeService.SelectDictDataByType(dictType);
		// Java 用 ObjectUtil.isNull 兜底为空列表，这里统一收敛
		return (data ?? new List<SysDictDataVo>()).ToCommonResult();
	}

	/// <summary>
	/// 新增字典数据。
	/// </summary>
	/// <param name="dict">字典数据参数</param>
	/// <returns>操作结果</returns>
	[Log("字典数据", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:dict:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysDictDataBo dict)
	{
		if (!await dictDataService.CheckDictDataUnique(dict))
		{
			return CommonResult.Fail($"新增字典数据'{dict.DictValue}'失败，字典键值已存在");
		}

		await dictDataService.InsertDictData(dict);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 修改字典数据。
	/// </summary>
	/// <param name="dict">字典数据参数</param>
	/// <returns>操作结果</returns>
	[Log("字典数据", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:dict:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysDictDataBo dict)
	{
		if (!await dictDataService.CheckDictDataUnique(dict))
		{
			return CommonResult.Fail($"修改字典数据'{dict.DictValue}'失败，字典键值已存在");
		}

		await dictDataService.UpdateDictData(dict);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 删除字典数据
	/// </summary>
	/// <param name="dictCodes">字典code串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("字典数据", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] dictCodes：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:dict:remove")]
	[HttpDelete("{dictCodes}")]
	public async Task<CommonResult> Remove([FromRoute] string dictCodes)
	{
		await dictDataService.DeleteDictDataByIds(ParseLongList(dictCodes));
		return CommonResult.Ok();
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(dictCodes)</c>）
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
