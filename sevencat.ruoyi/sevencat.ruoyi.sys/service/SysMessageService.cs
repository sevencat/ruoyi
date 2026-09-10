using System.Text.Json;
using Autofac.Annotation;
using MapsterMapper;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysMessageService(IFreeSql fsql, IMapper mapper)
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
}