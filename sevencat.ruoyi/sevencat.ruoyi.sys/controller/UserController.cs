using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.core.security.attr;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/system/user")]
public class UserController(
	IFreeSql fsql,
	IMapper mapper,
	LoginService loginService)
{
	[SaCheckPermission("system:client:query")]
	[HttpGet("getInfo")]
	public async Task<CommonResult<UserInfoVo>> GetInfo()
	{
		var loginUser = await loginService.GetLoginUser();

		var dbuser = await fsql.Select<TSysUser>().Where(x => x.UserId == loginUser.UserId).FirstAsync();
		var user = dbuser.MapTo<SysUserVo>(mapper);
		if (user == null)
		{
			return CommonResult.Fail("没有权限访问用户数据!");
		}

		var userInfoVo = new UserInfoVo();
		userInfoVo.User = user;
		userInfoVo.Permissions = loginUser.MenuPermission;
		userInfoVo.Roles = loginUser.RolePermission;
		return userInfoVo.ToCommonResult();
	}
}