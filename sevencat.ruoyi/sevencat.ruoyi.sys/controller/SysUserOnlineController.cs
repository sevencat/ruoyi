using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.log.attr;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 在线用户监控（对应 Java 的 <c>SysUserOnlineController</c>）
/// </summary>
[ApiController]
[Route("/api/monitor/online")]
public class SysUserOnlineController(SysUserOnlineService onlineService)
{
	/// <summary>
	/// 获取在线用户监控列表，并按 IP 或用户名条件过滤当前有效会话。
	/// </summary>
	/// <param name="ipaddr">IP地址</param>
	/// <param name="userName">用户名</param>
	/// <returns>在线用户分页数据</returns>
	[SaCheckPermission("monitor:online:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<UserOnlineDTO>>> List([FromQuery] string ipaddr,
		[FromQuery] string userName)
	{
		return BuildPage(await onlineService.SelectOnlineList(ipaddr, userName)).ToCommonResult();
	}

	/// <summary>
	/// 按 token 强制用户下线，适用于管理员踢除异常会话。
	/// </summary>
	/// <param name="tokenId">token值</param>
	/// <returns>操作结果</returns>
	[Log("在线用户", BusinessTypeEnum.Force)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[SaCheckPermission("monitor:online:forceLogout")]
	[HttpDelete("{tokenId}")]
	public async Task<CommonResult> ForceLogout([FromRoute] string tokenId)
	{
		await onlineService.ForceLogout(tokenId);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 获取当前登录用户的在线设备列表，仅返回当前账号仍有效的 token 会话。
	/// </summary>
	/// <returns>当前用户在线设备列表</returns>
	// Java 原注解 @GetMapping()：登录即可访问；未登录时 Java 会抛 NotLoginException，C# 端返回错误提示
	[HttpGet]
	public async Task<CommonResult<PageResult<UserOnlineDTO>>> GetInfo()
	{
		var list = await onlineService.SelectMyOnlineList();
		if (list == null)
		{
			return CommonResult.Fail("用户未登录");
		}

		return BuildPage(list).ToCommonResult();
	}

	/// <summary>
	/// 强退当前账号下指定在线设备，避免误踢其他账号的会话。
	/// </summary>
	/// <param name="tokenId">token值</param>
	/// <returns>操作结果</returns>
	[Log("在线设备", BusinessTypeEnum.Force)]
	// Java 原注解 @RepeatSubmit：C# 端无重复提交拦截，未实现
	[HttpDelete("myself/{tokenId}")]
	public async Task<CommonResult> Remove([FromRoute] string tokenId)
	{
		await onlineService.RemoveMySelf(tokenId);
		return CommonResult.Ok();
	}

	/// <summary>
	/// 构建列表分页结果（对应 Java 的 <c>PageResult.build(list)</c>，仅设置 rows 与 total）。
	/// </summary>
	/// <param name="list">列表数据</param>
	/// <returns>分页结果</returns>
	private static PageResult<UserOnlineDTO> BuildPage(List<UserOnlineDTO> list)
	{
		return new PageResult<UserOnlineDTO>
		{
			Rows = list,
			Total = list.Count,
		};
	}
}
