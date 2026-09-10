using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/resource/message")]
public class SysMessageController(
	IFreeSql fsql,
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
}