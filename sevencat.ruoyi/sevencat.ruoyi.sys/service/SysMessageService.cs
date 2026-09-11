using System.Text.Json;
using Autofac.Annotation;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.db;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.enums;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysMessageService(
	IFreeSql fsql,
	IMapper mapper,
	IIdGen idgen,
	SseManager sseManager)
{
	/// <summary>
	/// 全局广播用户标识（所有用户可见）
	/// </summary>
	private const string GLOBAL_USER_IDS = "0";

	/// <summary>
	/// 消息分类：系统消息
	/// </summary>
	public const string CATEGORY_SYSTEM = "system";

	/// <summary>
	/// 消息分类：通知公告
	/// </summary>
	public const string CATEGORY_NOTICE = "notice";

	/// <summary>
	/// 消息分类：工作流
	/// </summary>
	public const string CATEGORY_WORKFLOW = "workflow";

	/// <summary>
	/// 消息盒子每页展示最大条数
	/// </summary>
	private const int BOX_LIMIT = 100;

	/// <summary>
	/// 消息盒子展示消息天数（仅展示30天内）
	/// </summary>
	private const int BOX_DAYS = 30;

	/// <summary>
	/// 根据分类和用户ID查询消息列表
	/// 仅查询30天内、最多100条、按时间倒序
	/// </summary>
	/// <param name="category">消息分类</param>
	/// <param name="userId">用户ID</param>
	/// <returns>消息VO列表</returns>
	public async Task<List<SysMessageVo>> SelectMessageList(string category, long userId)
	{
		var list = await fsql.Select<TSysMessage>()
			.Where(m => m.Category == category)
			// 仅查询30天内消息
			.Where(m => m.CreateTime >= DateTime.Now.AddDays(-BOX_DAYS))
			// 全局消息 或 当前用户在接收人范围内（FIND_IN_SET 对应 MySQL 的 FIND_IN_SET）
			.Where("send_user_ids = @globalUserIds OR FIND_IN_SET(@uid, send_user_ids)",
				new { globalUserIds = GLOBAL_USER_IDS, uid = userId })
			.OrderByDescending(m => m.CreateTime)
			.OrderByDescending(m => m.MessageId)
			// 分页查询（只查第一页，最多100条）
			.Page(1, BOX_LIMIT)
			.ToListAsync();
		return list.Select(BuildVo).ToList();
	}

	
	/// <summary>
	/// 存储全局广播消息到数据库（对应 Java 的 <c>storeAll</c>）
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>回填消息ID后的消息推送体</returns>
	/// <remarks>
	/// Java 的 <c>publishAll</c> 为「<c>PushHelper.publishAll(storeAll(payload))</c>」，即先落库再推送在线用户；
	/// C# 端暂无 SSE / WebSocket 推送设施（PushHelper），故只保留落库部分，前端经消息盒子接口读取。
	/// </remarks>
	public async Task<PushPayloadDTO> PublishAll(PushPayloadDTO payload)
	{
		if (payload == null || !SupportsMessageBox(payload))
		{
			return payload;
		}

		var message = BuildMessage(payload);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人/创建时间
		// 消息盒子按创建时间（近30天）过滤，创建时间必须写入
		message.CreateBy ??= await LoginHelper.GetLoginUid();
		message.CreateTime ??= DateTime.Now;
		message.MessageId = idgen.NextId();

		await fsql.Insert(message).ExecuteAffrowsAsync();
		payload.MessageId = message.MessageId;
		await sseManager.BroadcastAsync(payload);
		return payload;
	}

	/// <summary>
	/// 消息实体转换为展示VO
	/// </summary>
	/// <param name="entity">消息实体</param>
	/// <returns>消息展示VO</returns>
	private SysMessageVo BuildVo(TSysMessage entity)
	{
		var vo = mapper.Map<SysMessageVo>(entity);
		vo.Data = ParseData(entity.DataJson);
		return vo;
	}

	/// <summary>
	/// 解析JSON数据字符串为对象
	/// </summary>
	/// <param name="dataJson">JSON字符串</param>
	/// <returns>解析后对象</returns>
	private static object ParseData(string dataJson)
	{
		if (string.IsNullOrWhiteSpace(dataJson))
		{
			return null;
		}

		return JsonSerializer.Deserialize<object>(dataJson);
	}

	/// <summary>
	/// 判断消息是否需要存入消息盒子（对应 Java 的 <c>supportsMessageBox</c>）
	/// 仅系统消息、通知消息需要存入
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>true 需要存入 false 不需要</returns>
	private static bool SupportsMessageBox(PushPayloadDTO payload)
	{
		// 仅消息/通知类型需要存入，排除LLM大模型消息
		var type = payload.Type;
		if (type != PushTypeEnum.Message.GetTypeValue() && type != PushTypeEnum.Notice.GetTypeValue())
		{
			return false;
		}

		return type != PushTypeEnum.Llm.GetTypeValue()
		       && payload.Source != PushSourceEnum.Llm.GetSourceValue();
	}

	/// <summary>
	/// 根据消息类型/来源自动解析消息分类（对应 Java 的 <c>resolveCategory</c>）
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>消息分类（system/notice/workflow）</returns>
	private static string ResolveCategory(PushPayloadDTO payload)
	{
		if (payload.Type == PushTypeEnum.Notice.GetTypeValue()
		    || payload.Source == PushSourceEnum.Notice.GetSourceValue())
		{
			return CATEGORY_NOTICE;
		}

		if (payload.Source == PushSourceEnum.Workflow.GetSourceValue())
		{
			return CATEGORY_WORKFLOW;
		}

		return CATEGORY_SYSTEM;
	}

	/// <summary>
	/// 根据消息分类自动生成消息标题（对应 Java 的 <c>resolveTitle</c>）
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>消息标题</returns>
	private static string ResolveTitle(PushPayloadDTO payload)
	{
		return ResolveCategory(payload) switch
		{
			CATEGORY_NOTICE => "通知公告消息",
			CATEGORY_WORKFLOW => "工作流消息",
			_ => "系统消息"
		};
	}

	/// <summary>
	/// 解析消息内容（对应 Java 的 <c>resolveContent</c>，从 data 中提取 noticeContent）
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>消息内容</returns>
	private static string ResolveContent(PushPayloadDTO payload)
	{
		if (payload.Data is IDictionary<string, object> map && map.TryGetValue("noticeContent", out var content))
		{
			return Convert.ToString(content);
		}

		return null;
	}

	/// <summary>
	/// 构建消息实体（对应 Java 的 <c>buildMessage</c>，用于数据库存储）
	/// </summary>
	/// <param name="payload">消息推送体</param>
	/// <returns>系统消息实体</returns>
	private static TSysMessage BuildMessage(PushPayloadDTO payload)
	{
		return new TSysMessage
		{
			// messageId 由 [Snowflake] 在插入时自动填充（对应 Java 的 IdGeneratorUtil.nextLongId()）
			Category = ResolveCategory(payload),
			Type = payload.Type,
			Source = payload.Source,
			Title = ResolveTitle(payload),
			Message = payload.Message,
			Content = ResolveContent(payload),
			DataJson = payload.Data == null ? null : JsonSerializer.Serialize(payload.Data),
			Path = payload.Path,
			// 全局广播（对应 Java 的 CollUtil.isEmpty(userIds) ? GLOBAL_USER_IDS : ...）
			SendUserIds = GLOBAL_USER_IDS
		};
	}
}