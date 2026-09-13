using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 岗位信息操作处理（对应 Java 的 <c>SysPostController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/post")]
public class SysPostController(
	SysPostService postService,
	SysDeptService deptService,
	ExcelDictFormatConverter dictFormatConverter)
{
	/// <summary>
	/// 岗位导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "岗位数据";

	/// <summary>
	/// 分页查询岗位列表。
	/// </summary>
	/// <param name="post">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>岗位分页结果</returns>
	[SaCheckPermission("system:post:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysPostVo>>> List([FromQuery] SysPostBo post,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await postService.SelectPagePostList(post, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 导出岗位列表。
	/// </summary>
	/// <param name="post">查询条件</param>
	[Log("岗位管理", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:post:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysPostBo post)
	{
		var list = await postService.SelectPostList(post);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（status 的 sys_normal_disable）
		await dictFormatConverter.ToExcelData(list);
		await list.WriteExcelToHttpAsync(ExcelSheetName);
	}

	/// <summary>
	/// 根据岗位编号获取详细信息
	/// </summary>
	/// <param name="postId">岗位ID</param>
	/// <returns>岗位详情</returns>
	[SaCheckPermission("system:post:query")]
	[HttpGet("{postId:long}")]
	public async Task<CommonResult<SysPostVo>> GetInfo([FromRoute] long postId)
	{
		return (await postService.SelectPostById(postId)).ToCommonResult();
	}

	/// <summary>
	/// 新增岗位。
	/// </summary>
	/// <param name="post">岗位参数</param>
	/// <returns>操作结果</returns>
	[Log("岗位管理", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:post:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysPostBo post)
	{
		if (!await postService.CheckPostNameUnique(post))
		{
			return CommonResult.Fail($"新增岗位'{post.PostName}'失败，岗位名称已存在");
		}

		if (!await postService.CheckPostCodeUnique(post))
		{
			return CommonResult.Fail($"新增岗位'{post.PostName}'失败，岗位编码已存在");
		}

		return ToAjax(await postService.InsertPost(post));
	}

	/// <summary>
	/// 修改岗位。
	/// </summary>
	/// <param name="post">岗位参数</param>
	/// <returns>操作结果</returns>
	[Log("岗位管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:post:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysPostBo post)
	{
		if (!await postService.CheckPostNameUnique(post))
		{
			return CommonResult.Fail($"修改岗位'{post.PostName}'失败，岗位名称已存在");
		}

		if (!await postService.CheckPostCodeUnique(post))
		{
			return CommonResult.Fail($"修改岗位'{post.PostName}'失败，岗位编码已存在");
		}

		if (SystemConstants.DISABLE.Equals(post.Status) && post.PostId.HasValue
		    && await postService.CountUserPostById(post.PostId.Value) > 0)
		{
			return CommonResult.Fail("该岗位下存在已分配用户，不能禁用!");
		}

		return ToAjax(await postService.UpdatePost(post));
	}

	/// <summary>
	/// 删除岗位
	/// </summary>
	/// <param name="postIds">岗位ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("岗位管理", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] postIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:post:remove")]
	[HttpDelete("{postIds}")]
	public async Task<CommonResult> Remove([FromRoute] string postIds)
	{
		return ToAjax(await postService.DeletePostByIds(ParseLongList(postIds)));
	}

	/// <summary>
	/// 获取岗位选择框列表
	/// </summary>
	/// <param name="postIds">岗位ID串，形如 1,2,3</param>
	/// <param name="deptId">部门id</param>
	/// <returns>岗位列表</returns>
	// Java 原注解 @RequestParam(required = false) Long[] postIds：Spring 支持 1,2,3 形式，
	// ASP.NET 对 string 参数会把重复键（postIds=1&postIds=2）合并为逗号串，故统一按字符串接收后切分
	[SaCheckPermission("system:post:query")]
	[HttpGet("optionselect")]
	public async Task<CommonResult<List<SysPostVo>>> OptionSelect([FromQuery] string postIds, [FromQuery] long? deptId)
	{
		var list = new List<SysPostVo>();
		if (deptId.HasValue)
		{
			// 对应 Java 的 ObjectUtil.isNotNull(deptId) 分支
			list = await postService.SelectPostList(new SysPostBo { DeptId = deptId });
		}
		else if (postIds.IsNotNullOrWhiteSpace())
		{
			list = await postService.SelectPostByIds(ParseLongList(postIds));
		}

		return list.ToCommonResult();
	}

	/// <summary>
	/// 获取岗位筛选用的部门树。
	/// </summary>
	/// <param name="dept">部门查询条件</param>
	/// <returns>部门树列表</returns>
	// Java 返回 cn.hutool.core.lang.tree.Tree<Long>，C# 端等价结构为 TreeSelectNode<TSysDept>
	[SaCheckPermission("system:post:list")]
	[HttpGet("deptTree")]
	public async Task<CommonResult<List<TreeSelectNode<TSysDept>>>> DeptTree([FromQuery] SysDeptBo dept)
	{
		return (await deptService.SelectDeptTreeList(dept)).ToCommonResult();
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(postIds)</c>）
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
