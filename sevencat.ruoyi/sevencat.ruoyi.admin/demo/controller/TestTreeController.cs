using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.demo.bo;
using sevencat.ruoyi.demo.service;
using sevencat.ruoyi.demo.vo;

namespace sevencat.ruoyi.demo.controller;

/// <summary>
/// 测试树表Controller（对应 Java 的 <c>TestTreeController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/demo/tree")]
public class TestTreeController(
	TestTreeService testTreeService,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 测试树表导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "测试树表";

	/// <summary>
	/// 查询测试树表列表
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>测试树表列表</returns>
	// Java 原注解 @Validated(QueryGroup.class)：C# 端无分组校验，需自行校验
	[SaCheckPermission("demo:tree:list")]
	[HttpGet("list")]
	public async Task<CommonResult<List<TestTreeVo>>> List([FromQuery] TestTreeBo bo)
	{
		var itemlst = await testTreeService.QueryListAsync(bo);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 导出测试树表列表
	/// </summary>
	/// <param name="bo">查询条件</param>
	[Log("测试树表", BusinessTypeEnum.Export)]
	[SaCheckPermission("demo:tree:export")]
	// Java 原注解 @GetMapping("/export") + ExcelBuilder.toResponse：C# 端改用 MiniExcel 写响应流
	[HttpGet("export")]
	public async Task Export([FromQuery] TestTreeBo bo)
	{
		var list = await testTreeService.QueryListAsync(bo);
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 获取测试树表详细信息
	/// </summary>
	/// <param name="id">测试树ID</param>
	/// <returns>测试树表详情</returns>
	[SaCheckPermission("demo:tree:query")]
	// Java 原注解 @NotNull(message = "主键不能为空") + @PathVariable("id")：C# 端由路由约束 {id:long} 保证
	[HttpGet("{id:long}")]
	public async Task<CommonResult<TestTreeVo>> GetInfo([FromRoute] long id)
	{
		return (await testTreeService.QueryByIdAsync(id)).ToCommonResult();
	}

	/// <summary>
	/// 新增测试树表
	/// </summary>
	/// <param name="bo">测试树表新增参数</param>
	/// <returns>操作结果</returns>
	[Log("测试树表", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	// Java 原注解 @Validated(AddGroup.class)：C# 端无分组校验，需自行校验
	[SaCheckPermission("demo:tree:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] TestTreeBo bo)
	{
		return ToAjax(await testTreeService.InsertByBoAsync(bo));
	}

	/// <summary>
	/// 修改测试树表
	/// </summary>
	/// <param name="bo">测试树表编辑参数</param>
	/// <returns>操作结果</returns>
	[Log("测试树表", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	// Java 原注解 @Validated(EditGroup.class)：C# 端无分组校验，需自行校验
	[SaCheckPermission("demo:tree:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] TestTreeBo bo)
	{
		return ToAjax(await testTreeService.UpdateByBoAsync(bo));
	}

	/// <summary>
	/// 删除测试树表
	/// </summary>
	/// <param name="ids">测试树ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("测试树表", BusinessTypeEnum.Delete)]
	// Java 原注解 @NotEmpty(message = "主键不能为空") + @DeleteMapping("/{ids}") Long[] ids：
	// Spring 支持 1,2,3 形式，ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("demo:tree:remove")]
	[HttpDelete("{ids}")]
	public async Task<CommonResult> Remove([FromRoute] string ids)
	{
		return ToAjax(await testTreeService.DeleteWithValidByIdsAsync(ParseLongList(ids), true));
	}

	/// <summary>
	/// 响应结果处理（对应 Java BaseController 的 <c>toAjax</c>）
	/// </summary>
	/// <param name="flag">是否成功</param>
	/// <returns>操作结果</returns>
	private static CommonResult ToAjax(bool flag)
	{
		return CommonResult.CreateDbUpdate(flag ? 1 : 0);
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(ids)</c>）
	/// </summary>
	/// <param name="value">逗号分隔的ID串</param>
	/// <returns>ID列表</returns>
	private static List<long> ParseLongList(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
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
