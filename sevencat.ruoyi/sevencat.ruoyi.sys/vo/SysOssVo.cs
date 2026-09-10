namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// OSS对象存储视图对象 sys_oss
/// </summary>
// Java 原注解 @AutoMapper(target = SysOss.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysOssVo
{
	/// <summary>
	/// 对象存储主键
	/// </summary>
	public long? OssId { get; set; }

	/// <summary>
	/// 文件名
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// 原名
	/// </summary>
	public string OriginalName { get; set; }

	/// <summary>
	/// 文件后缀名
	/// </summary>
	public string FileSuffix { get; set; }

	/// <summary>
	/// URL地址
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	public string Ext1 { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 上传人
	/// </summary>
	public long? CreateBy { get; set; }

	/// <summary>
	/// 上传人名称
	/// </summary>
	// Java 原注解 @Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "createBy")：C# 端无翻译注解，
	// 需在查询后自行按 CreateBy 回填名称
	public string CreateByName { get; set; }

	/// <summary>
	/// 服务商
	/// </summary>
	public string Service { get; set; }
}
