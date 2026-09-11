using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 菜单信息（对应 Java 的 <c>SysMenuController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/menu")]
public class SysMenuController(
	LoginService loginService,
	MenuService menuService)
{
	/// <summary>
	/// 获取路由信息
	/// </summary>
	/// <returns>当前用户可访问的路由信息</returns>
	[HttpGet("getRouters")]
	public async Task<CommonResult<List<RouterVo>>> GetRouters()
	{
		var lu = await loginService.GetLoginUser();
		var uid = lu.UserId.Value;
		var menus = await menuService.GetMenusByUid(uid);
		return menuService.BuildMenus(menus).ToCommonResult();
	}

	/// <summary>
	/// 查询菜单列表。
	/// </summary>
	/// <param name="menu">查询条件</param>
	/// <returns>菜单列表</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:list")]
	[HttpGet("list")]
	public async Task<CommonResult<List<SysMenuVo>>> List([FromQuery] SysMenuBo menu)
	{
		var uid = (await loginService.GetLoginUser()).UserId.Value;
		return (await menuService.SelectMenuList(menu, uid)).ToCommonResult();
	}

	/// <summary>
	/// 根据菜单编号获取详细信息
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>菜单详情</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:query")]
	[HttpGet("{menuId:long}")]
	public async Task<CommonResult<SysMenuVo>> GetInfo([FromRoute] long menuId)
	{
		return (await menuService.SelectMenuById(menuId)).ToCommonResult();
	}

	/// <summary>
	/// 获取菜单下拉树列表。
	/// </summary>
	/// <param name="menu">查询条件</param>
	/// <returns>菜单树</returns>
	[SaCheckPermission("system:menu:query")]
	[HttpGet("treeselect")]
	public async Task<CommonResult<List<MenuTreeSelectNode>>> Treeselect([FromQuery] SysMenuBo menu)
	{
		var uid = (await loginService.GetLoginUser()).UserId.Value;
		var menus = await menuService.SelectMenuList(menu, uid);
		return menuService.BuildMenuTreeSelect(menus).ToCommonResult();
	}

	/// <summary>
	/// 加载对应角色菜单列表树
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>角色菜单树及选中节点</returns>
	[SaCheckPermission("system:menu:query")]
	[HttpGet("roleMenuTreeselect/{roleId:long}")]
	public async Task<CommonResult<MenuTreeSelectVo>> RoleMenuTreeselect([FromRoute] long roleId)
	{
		var uid = (await loginService.GetLoginUser()).UserId.Value;
		var menus = await menuService.SelectMenuList(new SysMenuBo(), uid);
		var selectVo = new MenuTreeSelectVo
		{
			CheckedKeys = await menuService.SelectMenuListByRoleId(roleId),
			Menus = menuService.BuildMenuTreeSelect(menus)
		};
		return selectVo.ToCommonResult();
	}

	/// <summary>
	/// 新增菜单。
	/// </summary>
	/// <param name="menu">菜单参数</param>
	/// <returns>操作结果</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:add")]
	[Log("菜单管理", BusinessTypeEnum.Insert)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysMenuBo menu)
	{
		if (!await menuService.CheckMenuNameUnique(menu))
		{
			return CommonResult.Fail($"新增菜单'{menu.MenuName}'失败，菜单名称已存在");
		}

		if (SystemConstants.YES.Equals(menu.IsFrame) && !menu.Path.Ishttp())
		{
			return CommonResult.Fail($"新增菜单'{menu.MenuName}'失败，地址必须以http(s)://开头");
		}

		if (!await menuService.CheckRouteConfigUnique(menu))
		{
			return CommonResult.Fail($"新增菜单'{menu.MenuName}'失败，路由名称或地址已存在");
		}

		return ToAjax(await menuService.InsertMenu(menu));
	}

	/// <summary>
	/// 修改菜单。
	/// </summary>
	/// <param name="menu">菜单参数</param>
	/// <returns>操作结果</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:edit")]
	[Log("菜单管理", BusinessTypeEnum.Update)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysMenuBo menu)
	{
		if (!await menuService.CheckMenuNameUnique(menu))
		{
			return CommonResult.Fail($"修改菜单'{menu.MenuName}'失败，菜单名称已存在");
		}

		if (SystemConstants.YES.Equals(menu.IsFrame) && !menu.Path.Ishttp())
		{
			return CommonResult.Fail($"修改菜单'{menu.MenuName}'失败，地址必须以http(s)://开头");
		}

		if (menu.MenuId == menu.ParentId)
		{
			return CommonResult.Fail($"修改菜单'{menu.MenuName}'失败，上级菜单不能选择自己");
		}

		if (!await menuService.CheckRouteConfigUnique(menu))
		{
			return CommonResult.Fail($"修改菜单'{menu.MenuName}'失败，路由名称或地址已存在");
		}

		return ToAjax(await menuService.UpdateMenu(menu));
	}

	/// <summary>
	/// 删除菜单
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>操作结果</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:remove")]
	[Log("菜单管理", BusinessTypeEnum.Delete)]
	// Java 此处用 R.warn(...)（告警码 601）返回，C# 端 CommonResult 无 warn 语义，统一用 Fail 返回提示
	[HttpDelete("{menuId:long}")]
	public async Task<CommonResult> Remove([FromRoute] long menuId)
	{
		if (await menuService.HasChildByMenuId(menuId))
		{
			return CommonResult.Fail("存在子菜单,不允许删除");
		}

		if (await menuService.CheckMenuExistRole(menuId))
		{
			return CommonResult.Fail("菜单已分配,不允许删除");
		}

		return ToAjax(await menuService.DeleteMenuById(menuId));
	}

	/// <summary>
	/// 批量级联删除菜单
	/// </summary>
	/// <param name="menuIds">菜单ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	[SaCheckRole(SystemConstants.SUPER_ADMIN_ROLE_KEY)]
	[SaCheckPermission("system:menu:remove")]
	[Log("菜单管理", BusinessTypeEnum.Delete)]
	// Java 原注解 @PathVariable Long[] menuIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[HttpDelete("cascade/{menuIds}")]
	public async Task<CommonResult> CascadeRemove([FromRoute] string menuIds)
	{
		var menuIdList = ParseLongList(menuIds);
		if (await menuService.HasChildByMenuId(menuIdList))
		{
			return CommonResult.Fail("存在子菜单,不允许删除");
		}

		await menuService.DeleteMenuById(menuIdList);
		return CommonResult.Ok();
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>List.of(menuIds)</c>）
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
	/// 角色菜单列表树信息（对应 Java 控制器内的 <c>MenuTreeSelectVo</c> record）
	/// </summary>
	public class MenuTreeSelectVo
	{
		/// <summary>
		/// 选中菜单列表
		/// </summary>
		public List<long> CheckedKeys { get; set; } = [];

		/// <summary>
		/// 菜单下拉树结构列表
		/// </summary>
		public List<MenuTreeSelectNode> Menus { get; set; } = [];
	}
}
