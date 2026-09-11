using Microsoft.AspNetCore.Mvc;
using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

/// <summary>
/// 公告 信息操作处理（对应 Java 的 <c>SysNoticeController</c>）
/// </summary>
// Java 原注解 @Validated：C# 端无等价校验管线，需自行校验
[ApiController]
[Route("/api/system/notice")]
public class SysNoticeController(
	SysNoticeService noticeService,
	SysDictDataApi dictService,
	SysMessageService messageService)
{
	/// <summary>
	/// 分页查询通知公告列表。
	/// </summary>
	/// <param name="notice">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>公告分页结果</returns>
	[SaCheckPermission("system:notice:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysNoticeVo>>> List([FromQuery] SysNoticeBo notice,
		[FromQuery] PageQuery2 pageQuery)
	{
		return (await noticeService.SelectPageNoticeList(notice, pageQuery)).ToCommonResult();
	}

	/// <summary>
	/// 根据通知公告编号获取详细信息
	/// </summary>
	/// <param name="noticeId">公告ID</param>
	/// <returns>公告详情</returns>
	[SaCheckPermission("system:notice:query")]
	[HttpGet("{noticeId:long}")]
	public async Task<CommonResult<SysNoticeVo>> GetInfo([FromRoute] long noticeId)
	{
		return (await noticeService.SelectNoticeById(noticeId)).ToCommonResult();
	}

	/// <summary>
	/// 新增通知公告，并向消息盒子写入公告摘要。
	/// </summary>
	/// <param name="notice">公告参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "通知公告", businessType = BusinessType.INSERT) @RepeatSubmit：C# 端无操作日志切面与重复提交拦截，未实现
	[SaCheckPermission("system:notice:add")]
	[HttpPost]
	public async Task<CommonResult> Add([FromBody] SysNoticeBo notice)
	{
		var rows = await noticeService.InsertNotice(notice);
		if (rows <= 0)
		{
			// 对应 Java 的 R.fail()，其默认提示为「操作失败」
			return CommonResult.Fail("操作失败");
		}

		var type = await dictService.SelectDictLabel("sys_notice_type", notice.NoticeType);
		var data = new Dictionary<string, object>(6)
		{
			["noticeType"] = notice.NoticeType,
			["noticeTypeLabel"] = type,
			["noticeTitle"] = notice.NoticeTitle,
			["noticeId"] = notice.NoticeId,
			["noticeContent"] = notice.NoticeContent,
			["status"] = notice.Status
		};

		// Java 为 messageService.publishAll(...)，即「写入 sys_message + PushHelper 推送在线用户」；
		// C# 端暂无 SSE / WebSocket 推送设施，仅把公告写入消息盒子，前端通过消息盒子接口读取
		await messageService.StoreAll(PushPayloadDTO.Of(
			PushTypeEnum.Notice,
			PushSourceEnum.Notice,
			$"[{type}] {notice.NoticeTitle}",
			data,
			$"/system/notice?noticeId={notice.NoticeId}"));

		return CommonResult.Ok();
	}

	/// <summary>
	/// 修改通知公告。
	/// </summary>
	/// <param name="notice">公告参数</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "通知公告", businessType = BusinessType.UPDATE) @RepeatSubmit：C# 端无操作日志切面与重复提交拦截，未实现
	[SaCheckPermission("system:notice:edit")]
	[HttpPut]
	public async Task<CommonResult> Edit([FromBody] SysNoticeBo notice)
	{
		return ToAjax(await noticeService.UpdateNotice(notice));
	}

	/// <summary>
	/// 删除通知公告
	/// </summary>
	/// <param name="noticeIds">公告ID串，形如 1,2,3</param>
	/// <returns>操作结果</returns>
	// Java 原注解 @Log(title = "通知公告", businessType = BusinessType.DELETE)：C# 端无操作日志切面，未实现
	// Java 原注解 @PathVariable Long[] noticeIds：Spring 支持 1,2,3 形式，
	// ASP.NET 的 long[] 只认可重复键，这里按字符串接收后自行切分
	[SaCheckPermission("system:notice:remove")]
	[HttpDelete("{noticeIds}")]
	public async Task<CommonResult> Remove([FromRoute] string noticeIds)
	{
		return ToAjax(await noticeService.DeleteNoticeByIds(ParseLongList(noticeIds)));
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
	/// 解析逗号分隔的ID串（对应 Java 的 <c>Arrays.asList(noticeIds)</c>）
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
}
