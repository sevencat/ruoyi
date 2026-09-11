using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 社会化关系（对应 Java 的 <c>SysSocialController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/social")]
public class SysSocialController(SysSocialService socialService)
{
	/// <summary>
	/// 查询当前登录用户的社会化账号绑定列表。
	/// </summary>
	/// <returns>绑定关系列表</returns>
	[HttpGet("list")]
	public async Task<CommonResult<List<SysSocialVo>>> List()
	{
		// 对应 Java 的 LoginHelper.getUserId()
		var userId = (await LoginHelper.GetLoginUid()).Value;
		return (await socialService.QueryListByUserId(userId)).ToCommonResult();
	}
}
