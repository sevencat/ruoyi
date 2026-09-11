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
/// 操作日志记录（对应 Java 的 <c>SysOperLogController</c>）
/// </summary>
[ApiController]
[Route("/api/monitor/operlog")]
public class SysOperLogController(
	SysOperLogService operLogService,
	ExcelDictFormatConverter dictFormatConverter,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 操作日志导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "操作日志";

	/// <summary>
	/// 分页查询操作日志列表。
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>操作日志分页结果</returns>
	[SaCheckPermission("monitor:operlog:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysOperLogVo>>> List([FromQuery] SysOperLogBo bo,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await operLogService.SelectPageOperLogList(bo, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 导出操作日志列表。
	/// </summary>
	/// <param name="bo">查询条件</param>
	[Log("操作日志", BusinessTypeEnum.Export)]
	[SaCheckPermission("monitor:operlog:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysOperLogBo bo)
	{
		var list = await operLogService.SelectOperLogList(bo);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（businessType 的 sys_oper_type、status 的 sys_common_status 等）
		await dictFormatConverter.ToExcelData(list);
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 删除操作日志。
	/// </summary>
	/// <param name="operIds">日志主键串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @PathVariable Long[] operIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[Log("操作日志", BusinessTypeEnum.Delete)]
	[SaCheckPermission("monitor:operlog:remove")]
	[HttpDelete("{operIds}")]
	public async Task<CommonResult> Remove([FromRoute] string operIds)
	{
		return ToAjax(await operLogService.DeleteOperLogByIds(ParseLongList(operIds)));
	}

	/// <summary>
	/// 清空操作日志。
	/// </summary>
	/// <returns>操作结果</returns>
	// Java 原注解 @Lock4j：C# 端未引入分布式锁组件，如需并发保护请自行加锁
	[Log("操作日志", BusinessTypeEnum.Clean)]
	[SaCheckPermission("monitor:operlog:remove")]
	[HttpDelete("clean")]
	public async Task<CommonResult> Clean()
	{
		return ToAjax(await operLogService.CleanOperLog());
	}

	/// <summary>
	/// 查询操作日志详细。
	/// </summary>
	/// <param name="operId">日志主键</param>
	/// <returns>操作日志详情</returns>
	[SaCheckPermission("monitor:operlog:query")]
	[HttpGet("{operId:long}")]
	public async Task<CommonResult<SysOperLogVo>> GetInfo([FromRoute] long operId)
	{
		return (await operLogService.SelectOperLogById(operId)).ToCommonResult();
	}

	/// <summary>
	/// 响应结果处理（对应 Java BaseController 的 <c>toAjax</c>）
	/// </summary>
	/// <param name="rows">影响行数</param>
	/// <returns>操作结果</returns>
	private static CommonResult ToAjax(int rows)
	{
		return CommonResult.CreateDbUpdate(rows);
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(operIds)</c>）
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
