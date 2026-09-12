namespace sevencat.ruoyi.sys.dto;

/// <summary>
/// 附件扩展字段对象（存储在 SysOss.ext1 的 JSON 字符串中）
/// </summary>
/// <remarks>对应 Java 的 <c>SysOssExt</c>，JSON 使用 camelCase 命名（与 Java 序列化结果一致）。</remarks>
public class SysOssExt
{
	/// <summary>
	/// 所属业务类型（如 avatar、report、contract）
	/// </summary>
	public string BizType { get; set; }

	/// <summary>
	/// 文件大小（单位：字节）
	/// </summary>
	public long? FileSize { get; set; }

	/// <summary>
	/// 文件类型（MIME类型，如 image/png）
	/// </summary>
	public string ContentType { get; set; }

	/// <summary>
	/// 来源标识（如 userUpload、systemImport）
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// 上传 IP 地址，便于审计和追踪
	/// </summary>
	public string UploadIp { get; set; }

	/// <summary>
	/// 附件说明或备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 附件标签，如 ["图片", "证件"]
	/// </summary>
	public List<string> Tags { get; set; }

	/// <summary>
	/// 业务绑定ID（如某业务记录ID）
	/// </summary>
	public string RefId { get; set; }

	/// <summary>
	/// 绑定业务类型
	/// </summary>
	public string RefType { get; set; }

	/// <summary>
	/// 是否为临时文件，用于区分正式或待清理
	/// </summary>
	public bool? IsTemp { get; set; }

	/// <summary>
	/// 文件MD5值（可用于去重或校验）
	/// </summary>
	public string Md5 { get; set; }
}
