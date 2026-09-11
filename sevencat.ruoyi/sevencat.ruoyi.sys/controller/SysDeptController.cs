using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 部门信息（对应 Java 的 <c>SysDeptController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/dept")]
public class SysDeptController(SysDeptService deptService, SysPostService postService)
{
	/// <summary>
	/// 查询部门列表。
	/// </summary>
	/// <param name="dept">查询条件</param>
	/// <returns>部门列表</returns>
	[SaCheckPermission("system:dept:list")]
	[HttpGet("list")]
	public async Task<CommonResult<List<SysDeptVo>>> List([FromQuery] SysDeptBo dept)
	{
		var depts = await deptService.SelectDeptList(dept);
		return depts.ToCommonResult();
	}

	/// <summary>
	/// 查询部门列表（排除节点）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>过滤后的部门列表</returns>
	[SaCheckPermission("system:dept:list")]
	[HttpGet("list/exclude/{deptId:long}")]
	public async Task<CommonResult<List<SysDeptVo>>> ExcludeChild([FromRoute] long deptId)
	{
		var depts = await deptService.SelectDeptList(new SysDeptBo());
		// 排除自身与其所有下级部门（对应 Java 的 deptId.equals(...) || splitList(ancestors).contains(...)）
		depts.RemoveAll(d => d.DeptId == deptId || IsInAncestors(d.Ancestors, deptId));
		return depts.ToCommonResult();
	}

	/// <summary>
	/// 根据部门编号获取详细信息
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门详情</returns>
	[SaCheckPermission("system:dept:query")]
	[HttpGet("{deptId:long}")]
	public async Task<CommonResult<SysDeptVo>> GetInfo([FromRoute] long deptId)
	{
		await deptService.CheckDeptDataScope(deptId);
		return (await deptService.SelectDeptById(deptId)).ToCommonResult();
	}

	/// <summary>
	/// 新增部门。
	/// </summary>
	/// <param name="dept">部门参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "部门管理", businessType = BusinessType.INSERT) @RepeatSubmit：C# 端无操作日志切面与重复提交拦截，未实现
	[SaCheckPermission("system:dept:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysDeptBo dept)
	{
		if (!await deptService.CheckDeptNameUnique(dept))
		{
			return CommonResult.Fail($"新增部门'{dept.DeptName}'失败，部门名称已存在");
		}

		return ToAjax(await deptService.InsertDept(dept));
	}

	/// <summary>
	/// 修改部门。
	/// </summary>
	/// <param name="dept">部门参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "部门管理", businessType = BusinessType.UPDATE) @RepeatSubmit：C# 端无操作日志切面与重复提交拦截，未实现
	[SaCheckPermission("system:dept:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysDeptBo dept)
	{
		await deptService.CheckDeptDataScope(dept.DeptId);

		if (!await deptService.CheckDeptNameUnique(dept))
		{
			return CommonResult.Fail($"修改部门'{dept.DeptName}'失败，部门名称已存在");
		}

		if (dept.ParentId == dept.DeptId)
		{
			return CommonResult.Fail($"修改部门'{dept.DeptName}'失败，上级部门不能是自己");
		}

		if (SystemConstants.DISABLE.Equals(dept.Status))
		{
			if (await deptService.SelectNormalChildrenDeptById(dept.DeptId) > 0)
			{
				return CommonResult.Fail("该部门包含未停用的子部门!");
			}

			if (await deptService.CheckDeptExistUser(dept.DeptId))
			{
				return CommonResult.Fail("该部门下存在已分配用户，不能禁用!");
			}
		}

		return ToAjax(await deptService.UpdateDept(dept));
	}

	/// <summary>
	/// 删除部门
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "部门管理", businessType = BusinessType.DELETE)：C# 端无操作日志切面，未实现
	// Java 此处用 R.warn(...)（告警码 601）返回，C# 端 CommonResult 无 warn 语义，统一用 Fail 返回提示
	[SaCheckPermission("system:dept:remove")]
	[HttpDelete("{deptId:long}")]
	public async Task<CommonResult> Remove([FromRoute] long deptId)
	{
		if (SystemConstants.DEFAULT_DEPT_ID == deptId)
		{
			return CommonResult.Fail("默认部门,不允许删除");
		}

		if (await deptService.HasChildByDeptId(deptId))
		{
			return CommonResult.Fail("存在下级部门,不允许删除");
		}

		if (await deptService.CheckDeptExistUser(deptId))
		{
			return CommonResult.Fail("部门存在用户,不允许删除");
		}

		if (await postService.CountPostByDeptId(deptId) > 0)
		{
			return CommonResult.Fail("部门存在岗位,不允许删除");
		}

		await deptService.CheckDeptDataScope(deptId);
		return ToAjax(await deptService.DeleteDeptById(deptId));
	}

	/// <summary>
	/// 获取部门选择框列表
	/// </summary>
	/// <param name="deptIds">部门ID串，形如 1,2,3；为空时返回全部正常状态部门</param>
	/// <returns>部门列表</returns>
	// Java 原注解 @RequestParam(required = false) Long[] deptIds：Spring 支持 1,2,3 形式，
	// ASP.NET 对 string 参数会把重复键（deptIds=1&deptIds=2）合并为逗号串，故统一按字符串接收后切分
	[SaCheckPermission("system:dept:query")]
	[HttpGet("optionselect")]
	public async Task<CommonResult<List<SysDeptVo>>> OptionSelect([FromQuery] string deptIds)
	{
		return (await deptService.SelectDeptByIds(ParseLongList(deptIds))).ToCommonResult();
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>StringUtils.splitTo(value, Convert::toLong)</c>）
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
	/// 判断部门ID是否出现在祖级串中（对应 Java 的 <c>StringUtils.splitList(ancestors).contains(...)</c>）
	/// </summary>
	/// <param name="ancestors">祖级列表，如 0,100,101</param>
	/// <param name="deptId">目标部门ID</param>
	/// <returns>true 在祖级串中 false 不在</returns>
	private static bool IsInAncestors(string ancestors, long deptId)
	{
		return (ancestors ?? string.Empty)
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Contains(deptId.ToString());
	}
}
