using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/resource/message")]
public class SysMessageController(
	SseManager sseManager,
	LoginService loginService,
	SysMessageService messageService)
{
	/// <summary>
	/// 查询当前用户消息盒子数据
	/// 按系统消息、通知公告、工作流消息分类返回
	/// </summary>
	/// <returns>消息盒子数据</returns>
	[HttpGet("box")]
	public async Task<CommonResult<SysMessageBoxVo>> GetBox()
	{
		var userId = (await LoginHelper.GetLoginUid()).Value;
		var box = new SysMessageBoxVo
		{
			SystemList = await messageService.SelectMessageList(SysMessageService.CATEGORY_SYSTEM, userId),
			NoticeList = await messageService.SelectMessageList(SysMessageService.CATEGORY_NOTICE, userId),
			WorkflowList = await messageService.SelectMessageList(SysMessageService.CATEGORY_WORKFLOW, userId)
		};
		return box.ToCommonResult();
	}

	[HttpGet]
	public async Task SseConnect([FromQuery] string Authorization, CancellationToken cancellationToken)
	{
		var httpcontext = HttpUtil.GetCurrentHttpContext();
		// === 认证逻辑开始 ===
		if (string.IsNullOrEmpty(Authorization))
		{
			httpcontext.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}

		// 假设这里解析 Token 拿到了真实的 UserId
		var lu = await loginService.GetLoginUserByToken(Authorization);
		if (lu == null)
		{
			httpcontext.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}
		// === 认证逻辑结束 ===

		// 交给管理器接管，保持长连接
		var userId = lu.UserId.Value;
		await sseManager.RegisterClientAsync(userId, Authorization, httpcontext, cancellationToken);
	}
}