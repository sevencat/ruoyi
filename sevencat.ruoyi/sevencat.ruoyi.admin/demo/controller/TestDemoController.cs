using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.demo.bo;
using sevencat.ruoyi.demo.service;
using sevencat.ruoyi.demo.vo;

namespace sevencat.ruoyi.demo.controller;

/// <summary>
/// 测试单表Controller（对应 Java 的 <c>TestDemoController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/demo/demo")]
public class TestDemoController(
	TestDemoService testDemoService,
	IHttpContextAccessor httpCtxAccessor)
{
	/// <summary>
	/// 测试单表导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "测试单表";

	/// <summary>
	/// 查询测试单表列表
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>测试单表分页结果</returns>
	[SaCheckPermission("demo:demo:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<TestDemoVo>>> List([FromQuery] TestDemoBo bo,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await testDemoService.QueryPageListAsync(bo, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 自定义分页查询
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>测试单表分页结果</returns>
	// Java 端该方法走自定义 SQL（SELECT * FROM test_demo），与默认分页 SQL 一致，C# 端复用同一实现
	[SaCheckPermission("demo:demo:list")]
	[HttpGet("page")]
	public async Task<CommonResult<PageResult<TestDemoVo>>> Page([FromQuery] TestDemoBo bo,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await testDemoService.CustomPageListAsync(bo, pageQuery);
		return itemlst.ToCommonResult();
	}

	/// <summary>
	/// 导入数据
	/// </summary>
	/// <param name="file">导入文件</param>
	/// <returns>导入结果说明</returns>
	[Log("测试单表", BusinessTypeEnum.Import)]
	[SaCheckPermission("demo:demo:import")]
	// Java 原注解 @PostMapping(value = "/importData", consumes = MULTIPART_FORM_DATA_VALUE)
	// Java 通过 ExcelBuilder.read(...).validate(true).doRead() 读取，C# 端先用 MiniExcel 解析出列表，再交给 Service 校验、落库
	[HttpPost("importData")]
	public async Task<CommonResult> ImportData([FromForm] IFormFile file)
	{
		if (file == null)
		{
			return CommonResult.Fail("导入文件不能为空");
		}

		// MiniExcel 读取需要可随机访问的流，先整体拷进内存
		using var stream = new MemoryStream();
		await file.CopyToAsync(stream);
		stream.Position = 0;
		// MiniExcel 1.46 泛型 Query 用 hasHeader 指定首行为标题行
		var list = MiniExcel.Query<TestDemoImportVo>(stream, hasHeader: true).ToList();

		return CommonResult.Ok(await testDemoService.ImportAsync(list));
	}

	/// <summary>
	/// 导出测试单表列表
	/// </summary>
	/// <param name="bo">查询条件</param>
	[Log("测试单表", BusinessTypeEnum.Export)]
	[SaCheckPermission("demo:demo:export")]
	// Java 原注解 @PostMapping("/export") + ExcelBuilder.toResponse：C# 端改用 MiniExcel 写响应流
	[HttpPost("export")]
	public async Task Export([FromQuery] TestDemoBo bo)
	{
		var list = await testDemoService.QueryListAsync(bo);
		await ExcelResponseWriter.WriteAsync(httpCtxAccessor.HttpContext.Response, list, ExcelSheetName);
	}

	/// <summary>
	/// 获取测试单表详细信息
	/// </summary>
	/// <param name="id">测试ID</param>
	/// <returns>测试单表详情</returns>
	[SaCheckPermission("demo:demo:query")]
	// Java 原注解 @NotNull(message = "主键不能为空") + @PathVariable("id")：C# 端由路由约束 {id:long} 保证
	[HttpGet("{id:long}")]
	public async Task<CommonResult<TestDemoVo>> GetInfo([FromRoute] long id)
	{
		return (await testDemoService.QueryByIdAsync(id)).ToCommonResult();
	}

	/// <summary>
	/// 新增测试单表
	/// </summary>
	/// <param name="bo">测试单表新增参数</param>
	/// <returns>操作结果</returns>
	[Log("测试单表", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	// Java 原注解 @Validated(AddGroup.class) + ValidatorUtils.validate(bo, AddGroup.class)：C# 端无分组校验，需自行校验
	[SaCheckPermission("demo:demo:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] TestDemoBo bo)
	{
		return ToAjax(await testDemoService.InsertByBoAsync(bo));
	}

	/// <summary>
	/// 修改测试单表
	/// </summary>
	/// <param name="bo">测试单表编辑参数</param>
	/// <returns>操作结果</returns>
	[Log("测试单表", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	// Java 原注解 @Validated(EditGroup.class)：C# 端无分组校验，需自行校验
	[SaCheckPermission("demo:demo:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] TestDemoBo bo)
	{
		return ToAjax(await testDemoService.UpdateByBoAsync(bo));
	}

	/// <summary>
	/// 删除测试单表
	/// </summary>
	/// <param name="ids">测试ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("测试单表", BusinessTypeEnum.Delete)]
	// Java 原注解 @DeleteMapping("/{ids}") Long[] ids：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("demo:demo:remove")]
	[HttpDelete("{ids}")]
	public async Task<CommonResult> Remove([FromRoute] string ids)
	{
		return ToAjax(await testDemoService.DeleteWithValidByIdsAsync(ParseLongList(ids), true));
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
