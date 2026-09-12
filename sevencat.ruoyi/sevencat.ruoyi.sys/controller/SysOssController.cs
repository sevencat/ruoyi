using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 文件上传 控制层（对应 Java 的 <c>SysOssController</c>）
/// </summary>
/// <remarks>
/// Java 端 <c>@RequestMapping("/resource/oss")</c>，本工程统一加 <c>/api</c> 前缀。
/// 未移植 sys_oss_config 相关接口，存储统一走容器中已注入的 MinIO 客户端。
/// </remarks>
[ApiController]
[Route("/api/resource/oss")]
public class SysOssController(SysOssService ossService, IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 分页查询 OSS 列表
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>OSS 分页数据</returns>
	[SaCheckPermission("system:oss:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysOssVo>>> List([FromQuery] SysOssBo bo,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await ossService.SelectPageOssList(bo, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 按主键集合查询 OSS 列表
	/// </summary>
	/// <param name="ossIds">对象存储主键串，形如 1,2,3</param>
	/// <returns>OSS 集合</returns>
	// Java 原注解 @PathVariable Long[] ossIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:oss:query")]
	[HttpGet("listByIds/{ossIds}")]
	public async Task<CommonResult<List<SysOssVo>>> ListByIds([FromRoute] string ossIds)
	{
		return (await ossService.ListByIds(ParseLongList(ossIds))).ToCommonResult();
	}

	/// <summary>
	/// 上传文件
	/// </summary>
	/// <param name="file">上传的文件</param>
	/// <param name="ossExt">附件扩展信息 JSON，可空</param>
	/// <returns>上传结果（url / fileName / ossId）</returns>
	[Log("OSS对象存储", BusinessTypeEnum.Insert)]
	[SaCheckPermission("system:oss:upload")]
	[HttpPost("upload")]
	public async Task<CommonResult<SysOssUploadVo>> Upload([FromForm] IFormFile file, [FromForm] string ossExt)
	{
		var oss = await ossService.Upload(file, ParseOssExt(ossExt));
		return new SysOssUploadVo(oss.Url, oss.OriginalName, oss.OssId?.ToString()).ToCommonResult();
	}

	/// <summary>
	/// 下载文件
	/// </summary>
	/// <param name="ossId">对象存储主键</param>
	[SaCheckPermission("system:oss:download")]
	[HttpGet("download/{ossId:long}")]
	public async Task Download([FromRoute] long ossId)
	{
		var file = await ossService.Download(ossId);
		var response = httpCtxAccessor.HttpContext.Response;
		var encodedFileName = Uri.EscapeDataString(file.FileName ?? string.Empty);

		// 对应 Java 的两个响应头：Content-Disposition 给浏览器，download-filename 给前端 download 插件读取
		response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition,download-filename";
		response.Headers.ContentDisposition =
			$"attachment;filename={encodedFileName};filename*=utf-8''{encodedFileName}";
		response.Headers["download-filename"] = encodedFileName;
		response.ContentType = file.ContentType;
		response.ContentLength = file.Data.Length;
		await response.Body.WriteAsync(file.Data);
	}

	/// <summary>
	/// 批量删除 OSS（同时删除对象存储中的文件）
	/// </summary>
	/// <param name="ossIds">对象存储主键串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("OSS对象存储", BusinessTypeEnum.Delete)]
	[SaCheckPermission("system:oss:remove")]
	[HttpDelete("{ossIds}")]
	public async Task<CommonResult> Remove([FromRoute] string ossIds)
	{
		return ToAjax(await ossService.DeleteWithValidByIds(ParseLongList(ossIds), true));
	}

	/// <summary>
	/// 解析上传时携带的扩展信息 JSON（对应 Java 的 <c>JsonUtils.parseObject(ossExtJson, SysOssExt.class)</c>）
	/// </summary>
	/// <param name="ossExtJson">扩展信息 JSON</param>
	/// <returns>扩展信息对象，解析失败或为空时返回 null</returns>
	private static SysOssExt ParseOssExt(string ossExtJson)
	{
		if (ossExtJson.IsNullOrWhiteSpace())
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<SysOssExt>(ossExtJson, JsonSerializerOptions.Web);
		}
		catch (JsonException)
		{
			// 非法 JSON 时按未传处理，避免直接 500
			return null;
		}
	}

	/// <summary>
	/// 响应结果处理（对应 Java BaseController 的 <c>toAjax(boolean)</c>）
	/// </summary>
	/// <param name="flag">是否成功</param>
	/// <returns>操作结果</returns>
	private static CommonResult ToAjax(bool flag)
	{
		return CommonResult.CreateDbUpdate(flag ? 1 : 0);
	}

	/// <summary>
	/// 解析逗号分隔的ID串
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

	/// <summary>
	/// 上传返回对象（对应 Java 的 <c>SysOssController.SysOssUploadVo</c>）
	/// </summary>
	/// <param name="Url">访问地址</param>
	/// <param name="FileName">原始文件名</param>
	/// <param name="OssId">对象存储主键（字符串形式，避免前端大整数精度丢失）</param>
	public record SysOssUploadVo(string Url, string FileName, string OssId);
}
