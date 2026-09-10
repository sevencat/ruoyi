namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// OSS对象存储分页查询对象 sys_oss
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysOss.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysOssBo
{
	/// <summary>
	/// ossId
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
	/// 服务商
	/// </summary>
	public string Service { get; set; }

	/// <summary>
	/// 创建者
	/// </summary>
	public long? CreateBy { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}
