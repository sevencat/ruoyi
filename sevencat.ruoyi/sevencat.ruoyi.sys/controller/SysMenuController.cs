using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/system/menu")]
public class SysMenuController(
	LoginService loginService,
	MenuService menuService)
{
	[HttpGet("getRouters")]
	public async Task<CommonResult<List<RouterVo>>> GetRouters()
	{
		var lu = await loginService.GetLoginUser();
		var uid = lu.UserId.Value;
		var menus = await menuService.GetMenusByUid(uid);
		return menuService.BuildMenus(menus).ToCommonResult();
	}
}