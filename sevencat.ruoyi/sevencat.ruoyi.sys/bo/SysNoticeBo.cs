namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 通知公告业务对象 sys_notice
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysNotice.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@Xss/@NotBlank/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysNoticeBo
{
	/// <summary>
	/// 公告ID
	/// </summary>
	public long? NoticeId { get; set; }

	/// <summary>
	/// 公告标题
	/// </summary>
	// Java 原注解 @Xss(message = "公告标题不能包含脚本字符")：C# 端无 XSS 校验注解，需自行过滤脚本字符
	// Java 原注解 @NotBlank(message = "公告标题不能为空")
	// Java 原注解 @Size(min = 0, max = 50, message = "公告标题不能超过{max}个字符")
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
	/// 创建人名称
	/// </summary>
	public string CreateByName { get; set; }
}
