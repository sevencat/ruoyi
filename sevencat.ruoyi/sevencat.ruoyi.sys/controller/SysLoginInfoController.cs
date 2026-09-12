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
/// 系统访问记录（对应 Java 的 <c>SysLoginInfoController</c>）
/// </summary>
// Java 端还有 @SaCheckPermission("monitor:logininfor:unlock") 的账号解锁接口，
// 用于清除 pwd_err_cnt 缓存；本项目登录暂无密码重试锁定逻辑，故未实现
[ApiController]
[Route("/api/monitor/logininfo")]
public class SysLoginInfoController(
	SysLoginInfoService loginInfoService,
	ExcelDictFormatConverter dictFormatConverter,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 登录日志导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "登录日志";

	/// <summary>
	/// 分页查询系统访问记录列表。
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>访问记录分页结果</returns>
	[SaCheckPermission("monitor:logininfor:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysLoginInfoVo>>> List([FromQuery] SysLoginInfoBo bo,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await loginInfoService.SelectPageLoginInfoList(bo, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 导出系统访问记录列表。
	/// </summary>
	/// <param name="bo">查询条件</param>
	[Log("登录日志", BusinessTypeEnum.Export)]
	[SaCheckPermission("monitor:logininfor:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysLoginInfoBo bo)
	{
		var list = await loginInfoService.SelectLoginInfoList(bo);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（deviceType 的 sys_device_type、status 的 sys_common_status）
		await dictFormatConverter.ToExcelData(list);
		await ExcelHttpExt.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 删除系统访问记录。
	/// </summary>
	/// <param name="infoIds">访问ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @PathVariable Long[] infoIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[Log("登录日志", BusinessTypeEnum.Delete)]
	[SaCheckPermission("monitor:logininfor:remove")]
	[HttpDelete("{infoIds}")]
	public async Task<CommonResult> Remove([FromRoute] string infoIds)
	{
		return ToAjax(await loginInfoService.DeleteLoginInfoByIds(ParseLongList(infoIds)));
	}

	/// <summary>
	/// 清空系统访问记录。
	/// </summary>
	/// <returns>操作结果</returns>
	[Log("登录日志", BusinessTypeEnum.Clean)]
	[SaCheckPermission("monitor:logininfor:remove")]
	[HttpDelete("clean")]
	public async Task<CommonResult> Clean()
	{
		return ToAjax(await loginInfoService.CleanLoginInfo());
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(infoIds)</c>）
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
