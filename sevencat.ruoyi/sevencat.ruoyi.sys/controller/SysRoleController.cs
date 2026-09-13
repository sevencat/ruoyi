using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
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
/// 角色信息操作处理（对应 Java 的 <c>SysRoleController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/role")]
public class SysRoleController(
	SysRoleService roleService,
	SysUserService userService,
	SysDeptService deptService,
	ExcelDictFormatConverter dictFormatConverter)
{
	/// <summary>
	/// 角色导出使用的 sheet 名
	/// </summary>
	private const string ExcelSheetName = "角色数据";

	/// <summary>
	/// 分页查询角色列表。
	/// </summary>
	/// <param name="role">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>角色分页结果</returns>
	[SaCheckPermission("system:role:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysRoleVo>>> List([FromQuery] SysRoleBo role,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await roleService.SelectPageRoleList(role, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 导出角色信息列表。
	/// </summary>
	/// <param name="role">查询条件</param>
	[Log("角色管理", BusinessTypeEnum.Export)]
	[SaCheckPermission("system:role:export")]
	[HttpPost("export")]
	public async Task Export([FromQuery] SysRoleBo role)
	{
		var list = await roleService.SelectRoleList(role);
		// 字典转换：对应 Java VO 上 @ExcelDictFormat 的转换器（status / dataScope）
		await dictFormatConverter.ToExcelData(list);
		await list.WriteExcelToHttpAsync(ExcelSheetName);
	}

	/// <summary>
	/// 根据角色编号获取详细信息
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>角色详情</returns>
	[SaCheckPermission("system:role:query")]
	[HttpGet("{roleId:long}")]
	public async Task<CommonResult<SysRoleVo>> GetInfo([FromRoute] long roleId)
	{
		await roleService.CheckRoleDataScope(roleId);
		return (await roleService.SelectRoleById(roleId)).ToCommonResult();
	}

	/// <summary>
	/// 新增角色。
	/// </summary>
	/// <param name="role">角色参数</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysRoleBo role)
	{
		await roleService.CheckRoleAllowed(role);
		if (!await roleService.CheckRoleNameUnique(role))
		{
			return CommonResult.Fail($"新增角色'{role.RoleName}'失败，角色名称已存在");
		}

		if (!await roleService.CheckRoleKeyUnique(role))
		{
			return CommonResult.Fail($"新增角色'{role.RoleName}'失败，角色权限已存在");
		}

		return ToAjax(await roleService.InsertRole(role));
	}

	/// <summary>
	/// 修改角色基础信息（不包含菜单权限、数据权限）。
	/// </summary>
	/// <param name="role">角色参数</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysRoleBo role)
	{
		await roleService.CheckRoleAllowed(role);
		await roleService.CheckRoleDataScope(role.RoleId);
		if (!await roleService.CheckRoleNameUnique(role))
		{
			return CommonResult.Fail($"修改角色'{role.RoleName}'失败，角色名称已存在");
		}

		if (!await roleService.CheckRoleKeyUnique(role))
		{
			return CommonResult.Fail($"修改角色'{role.RoleName}'失败，角色权限已存在");
		}

		if (await roleService.UpdateRoleBaseInfo(role) > 0)
		{
			// Java 发布 OnlineUserCleanEvent.byRole(roleId) 清理在线用户；C# 端无事件总线，未实现
			return CommonResult.Ok();
		}

		return CommonResult.Fail($"修改角色'{role.RoleName}'失败，请联系管理员");
	}

	/// <summary>
	/// 修改角色权限信息（菜单权限 + 数据权限）。
	/// </summary>
	/// <param name="role">角色参数</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut("permission")]
	public async Task<CommonResult> EditPermission([FromBody] SysRoleBo role)
	{
		await roleService.CheckRoleAllowed(role);
		await roleService.CheckRoleDataScope(role.RoleId);
		if (await roleService.UpdateRolePermission(role) > 0)
		{
			// Java 发布 OnlineUserCleanEvent.byRole(roleId) 清理在线用户；C# 端无事件总线，未实现
			return CommonResult.Ok();
		}

		return CommonResult.Fail($"修改角色'{role.RoleName}'权限失败，请联系管理员");
	}

	/// <summary>
	/// 修改角色状态。
	/// </summary>
	/// <param name="role">角色参数</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut("changeStatus")]
	public async Task<CommonResult> ChangeStatus([FromBody] SysRoleBo role)
	{
		await roleService.CheckRoleAllowed(role);
		await roleService.CheckRoleDataScope(role.RoleId);
		if (await roleService.UpdateRoleStatus(role.RoleId, role.Status) > 0)
		{
			// Java 发布 OnlineUserCleanEvent.byRole(roleId) 清理在线用户；C# 端无事件总线，未实现
			return CommonResult.Ok();
		}

		return CommonResult.Fail($"修改角色'{role.RoleName}'状态失败，请联系管理员");
	}

	/// <summary>
	/// 删除角色
	/// </summary>
	/// <param name="roleIds">角色ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] roleIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:role:remove")]
	[HttpDelete("{roleIds}")]
	public async Task<CommonResult> Remove([FromRoute] string roleIds)
	{
		return ToAjax(await roleService.DeleteRoleByIds(ParseLongList(roleIds)));
	}

	/// <summary>
	/// 获取角色选择框列表
	/// </summary>
	/// <param name="roleIds">角色ID串，形如 1,2,3</param>
	/// <returns>角色列表</returns>
	// Java 原注解 @RequestParam(required = false) Long[] roleIds：Spring 支持 1,2,3 形式，
	// ASP.NET 对 string 参数会把重复键（roleIds=1&roleIds=2）合并为逗号串，故统一按字符串接收后切分
	[SaCheckPermission("system:role:query")]
	[HttpGet("optionselect")]
	public async Task<CommonResult<List<SysRoleVo>>> Optionselect([FromQuery] string roleIds)
	{
		return (await roleService.SelectRoleByIds(ParseLongList(roleIds))).ToCommonResult();
	}

	/// <summary>
	/// 查询已分配用户角色列表。
	/// </summary>
	/// <param name="user">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页结果</returns>
	[SaCheckPermission("system:role:list")]
	[HttpGet("authUser/allocatedList")]
	public async Task<CommonResult<PageResult<SysUserVo>>> AllocatedList([FromQuery] SysUserBo user,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await userService.SelectAllocatedList(user, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 查询未分配用户角色列表。
	/// </summary>
	/// <param name="user">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页结果</returns>
	[SaCheckPermission("system:role:list")]
	[HttpGet("authUser/unallocatedList")]
	public async Task<CommonResult<PageResult<SysUserVo>>> UnallocatedList([FromQuery] SysUserBo user,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await userService.SelectUnallocatedList(user, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 取消授权用户。
	/// </summary>
	/// <param name="userRole">用户角色关系</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Grant)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut("authUser/cancel")]
	public async Task<CommonResult> CancelAuthUser([FromBody] SysUserRoleBo userRole)
	{
		return ToAjax(await roleService.DeleteAuthUser(userRole));
	}

	/// <summary>
	/// 批量取消授权用户
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <param name="userIds">用户ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Grant)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut("authUser/cancelAll")]
	public async Task<CommonResult> CancelAuthUserAll([FromQuery] long roleId, [FromQuery] string userIds)
	{
		return ToAjax(await roleService.DeleteAuthUsers(roleId, ParseLongList(userIds)));
	}

	/// <summary>
	/// 批量选择用户授权
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <param name="userIds">用户ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[Log("角色管理", BusinessTypeEnum.Grant)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("system:role:edit")]
	[HttpPut("authUser/selectAll")]
	public async Task<CommonResult> SelectAuthUserAll([FromQuery] long roleId, [FromQuery] string userIds)
	{
		await roleService.CheckRoleDataScope(roleId);
		return ToAjax(await roleService.InsertAuthUsers(roleId, ParseLongList(userIds)));
	}

	/// <summary>
	/// 获取对应角色部门树列表
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>角色部门树信息</returns>
	[SaCheckPermission("system:role:list")]
	[HttpGet("deptTree/{roleId:long}")]
	public async Task<CommonResult<DeptTreeSelectVo>> RoleDeptTreeselect([FromRoute] long roleId)
	{
		var selectVo = new DeptTreeSelectVo
		{
			CheckedKeys = await deptService.SelectDeptListByRoleId(roleId),
			// Java 的 deptService.selectDeptTreeList(new SysDeptBo())
			Depts = await deptService.SelectDeptTreeList(new SysDeptBo())
		};
		return selectVo.ToCommonResult();
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>List.of(roleIds)</c>）
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
	/// 角色部门列表树信息（对应 Java 控制器内的 <c>DeptTreeSelectVo</c> record）
	/// </summary>
	public class DeptTreeSelectVo
	{
		/// <summary>
		/// 选中部门列表
		/// </summary>
		public List<long> CheckedKeys { get; set; }

		/// <summary>
		/// 下拉树结构列表
		/// </summary>
		public List<TreeSelectNode<TSysDept>> Depts { get; set; }
	}
}
