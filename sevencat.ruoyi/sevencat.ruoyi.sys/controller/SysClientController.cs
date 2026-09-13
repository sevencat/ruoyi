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
/// 客户端管理（对应 Java 的 <c>SysClientController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/client")]
public class SysClientController(
	SysClientService clientService,
	ExcelDictFormatConverter dictFormatConverter)
{
	/// <summary>
	/// 客户端导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "客户端管理";

	/// <summary>
	/// 分页查询客户端管理列表。
	/// </summary>
	/// <param name="client">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>客户端分页数据</returns>
	[SaCheckPermission("system:client:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysClientVo>>> List([FromQuery] SysClientBo client,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await clientService.SelectPageClientList(client, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 导出客户端管理列表，便于离线审计与配置核查。
	/// </summary>
	/// <param name="client">查询条件</param>
	[Log("客户端管理", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:client:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysClientBo client)
	{
		var list = await clientService.SelectClientList(client);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（status 的 0=正常,1=停用）
		await dictFormatConverter.ToExcelData(list);
		await list.WriteExcelToHttpAsync(ExcelSheetName);
	}

	/// <summary>
	/// 获取单个客户端的详细配置信息。
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>客户端详情</returns>
	[SaCheckPermission("system:client:query")]
	[HttpGet("{id:long}")]
	public async Task<CommonResult<SysClientVo>> GetInfo([FromRoute] long id)
	{
		return (await clientService.QueryById(id)).ToCommonResult();
	}

	/// <summary>
	/// 新增客户端配置，入库前先校验客户端 key 是否唯一。
	/// </summary>
	/// <param name="client">客户端信息</param>
	/// <returns>操作结果</returns>
	[Log("客户端管理", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:client:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysClientBo client)
	{
		if (!await clientService.CheckClientKeyUnique(client))
		{
			return CommonResult.Fail($"新增客户端'{client.ClientKey}'失败，客户端key已存在");
		}

		return ToAjax(await clientService.InsertByBo(client));
	}

	/// <summary>
	/// 修改客户端配置，避免重复占用同一个客户端 key。
	/// </summary>
	/// <param name="client">客户端信息</param>
	/// <returns>操作结果</returns>
	[Log("客户端管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:client:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysClientBo client)
	{
		if (!await clientService.CheckClientKeyUnique(client))
		{
			return CommonResult.Fail($"修改客户端'{client.ClientKey}'失败，客户端key已存在");
		}

		return ToAjax(await clientService.UpdateByBo(client));
	}

	/// <summary>
	/// 修改客户端启停状态。
	/// </summary>
	/// <param name="client">客户端状态信息</param>
	/// <returns>操作结果</returns>
	[Log("客户端管理", BusinessTypeEnum.Update)]
	[SaCheckPermission("system:client:edit")]
	[HttpPut("changeStatus")]
	public async Task<CommonResult> ChangeStatus([FromBody] SysClientBo client)
	{
		return ToAjax(await clientService.UpdateClientStatus(client.ClientId, client.Status));
	}

	/// <summary>
	/// 批量删除客户端配置。
	/// </summary>
	/// <param name="ids">主键串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("客户端管理", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] ids：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:client:remove")]
	[HttpDelete("{ids}")]
	public async Task<CommonResult> Remove([FromRoute] string ids)
	{
		return ToAjax(await clientService.DeleteWithValidByIds(ParseLongList(ids), true));
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
	/// 响应结果处理（对应 Java BaseController 的 <c>toAjax(int)</c>）
	/// </summary>
	/// <param name="rows">影响行数</param>
	/// <returns>操作结果</returns>
	private static CommonResult ToAjax(int rows)
	{
		return CommonResult.CreateDbUpdate(rows);
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>List.of(ids)</c>）
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