namespace sevencat.ruoyi.sys.entity.dto;

/// <summary>
/// OSS 文件简要信息对象
/// </summary>
public class OssDTO
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
}
