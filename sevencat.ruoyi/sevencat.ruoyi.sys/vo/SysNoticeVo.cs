namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 通知公告视图对象 sys_notice
/// </summary>
// Java 原注解 @AutoMapper(target = SysNotice.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysNoticeVo
{
	/// <summary>
	/// 公告ID
	/// </summary>
	public long? NoticeId { get; set; }

	/// <summary>
	/// 公告标题
	/// </summary>
	public string NoticeTitle { get; set; }

	/// <summary>
	/// 公告类型（1通知 2公告）
	/// </summary>
	public string NoticeType { get; set; }

	/// <summary>
	/// 公告内容
	/// </summary>
	public string NoticeContent { get; set; }

	/// <summary>
	/// 公告状态（0正常 1关闭）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 创建者
	/// </summary>
	public long? CreateBy { get; set; }

	/// <summary>
	/// 创建人名称
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "createBy")：C# 端无翻译注解，
	// 需在查询后自行按 CreateBy 回填名称
	public string CreateByName { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }
}
