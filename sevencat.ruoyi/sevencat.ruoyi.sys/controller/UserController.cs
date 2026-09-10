using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
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
	LoginService loginService,
	SysDeptService deptService,
	SysUserService userService)
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
	
	/// <summary>
	/// 获取用户筛选用的部门树。
	/// </summary>
	/// <param name="dept">部门查询条件</param>
	/// <returns>部门树列表</returns>
	[SaCheckPermission("system:user:list")]
	[HttpGet("deptTree")]
	public async Task<CommonResult<List<TreeSelectNode<TSysDept>>>> DeptTree([FromQuery] SysDeptBo dept)
	{
		return (await deptService.SelectDeptTreeList(dept)).ToCommonResult();
	}


	/// <summary>
	/// 分页查询用户列表。
	/// </summary>
	/// <param name="user">用户查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页列表</returns>
	[SaCheckPermission("system:user:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysUserVo>>> List([FromQuery] SysUserBo user,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await userService.SelectPageUserList(user, pageQuery);
		return itemlst.ToCommonResult();
	}
}