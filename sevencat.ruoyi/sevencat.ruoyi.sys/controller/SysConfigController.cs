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
/// 参数配置 信息操作处理（对应 Java 的 <c>SysConfigController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/config")]
public class SysConfigController(
	SysConfigService configService,
	ExcelDictFormatConverter dictFormatConverter,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 参数配置导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "参数数据";

	/// <summary>
	/// 分页查询参数配置列表。
	/// </summary>
	/// <param name="config">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>参数配置分页结果</returns>
	[SaCheckPermission("system:config:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysConfigVo>>> List([FromQuery] SysConfigBo config,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await configService.SelectPageConfigList(config, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 导出参数配置列表。
	/// </summary>
	/// <param name="config">查询条件</param>
	[Log("参数管理", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:config:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysConfigBo config)
	{
		var list = await configService.SelectConfigList(config);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（configType 的 sys_yes_no）
		await dictFormatConverter.ToExcelData(list);
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 根据参数编号获取详细信息
	/// </summary>
	/// <param name="configId">参数ID</param>
	/// <returns>参数配置详情</returns>
	[SaCheckPermission("system:config:query")]
	[HttpGet("{configId:long}")]
	public async Task<CommonResult<SysConfigVo>> GetInfo([FromRoute] long configId)
	{
		return (await configService.SelectConfigById(configId)).ToCommonResult();
	}

	/// <summary>
	/// 根据参数键名查询参数值
	/// </summary>
	/// <param name="configKey">参数Key</param>
	/// <returns>参数值</returns>
	[HttpGet("configKey/{configKey}")]
	public async Task<CommonResult<string>> GetConfigKey([FromRoute] string configKey)
	{
		return (await configService.SelectConfigByKey(configKey)).ToCommonResult();
	}

	/// <summary>
	/// 新增参数配置。
	/// </summary>
	/// <param name="config">参数配置</param>
	/// <returns>操作结果</returns>
	// Java 原注解为裸 @Log（未指定 title/businessType），这里按模块补齐标题与业务类型，便于日志页面查看
	[Log("参数管理", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:config:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysConfigBo config)
	{
		if (!await configService.CheckConfigKeyUnique(config))
		{
			return CommonResult.Fail($"新增参数'{config.ConfigName}'失败，参数键名已存在");
		}

		await configService.InsertConfig(config);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 修改参数配置。
	/// </summary>
	/// <param name="config">参数配置</param>
	/// <returns>操作结果</returns>
	// Java 原注解为裸 @Log（未指定 title/businessType），这里按模块补齐标题与业务类型，便于日志页面查看
	[Log("参数管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:config:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysConfigBo config)
	{
		if (!await configService.CheckConfigKeyUnique(config))
		{
			return CommonResult.Fail($"修改参数'{config.ConfigName}'失败，参数键名已存在");
		}

		await configService.UpdateConfig(config);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 根据参数键名修改参数配置。
	/// </summary>
	/// <param name="config">参数配置</param>
	/// <returns>操作结果</returns>
	// Java 原注解为裸 @Log（未指定 title/businessType），这里按模块补齐标题与业务类型，便于日志页面查看
	[Log("参数管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:config:edit")]
	[HttpPut("updateByKey")]
	public async Task<CommonResult> UpdateByKey([FromBody] SysConfigBo config)
	{
		await configService.UpdateConfig(config);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 删除参数配置
	/// </summary>
	/// <param name="configIds">参数ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @PathVariable Long[] configIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:config:remove")]
	[HttpDelete("{configIds}")]
	public async Task<CommonResult> Remove([FromRoute] string configIds)
	{
		await configService.DeleteConfigByIds(ParseLongList(configIds));
		return CommonResult.Ok();
	}

	/// <summary>
	/// 刷新参数缓存。
	/// </summary>
	/// <returns>操作结果</returns>
	[SaCheckPermission("system:config:remove")]
	[HttpDelete("refreshCache")]
	public async Task<CommonResult> RefreshCache()
	{
		await configService.ResetConfigCache();
		return CommonResult.Ok();
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(configIds)</c>）
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
