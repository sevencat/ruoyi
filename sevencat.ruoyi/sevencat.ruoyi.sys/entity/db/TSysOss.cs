using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// OSS对象存储表 sys_oss
/// </summary>
[Table(Name = "sys_oss")]
public class TSysOss : TBaseEntity
{
	/// <summary>
	/// 对象存储主键
	/// </summary>
	[Column(Name = "oss_id", IsPrimary = true)]
	[Snowflake]
	public long OssId { get; set; }

	/// <summary>
	/// 文件名（对象键，如 2026/09/12/xxxxxxxx.png）
	/// </summary>
	[Column(Name = "file_name", StringLength = 255, IsNullable = false)]
	public string FileName { get; set; }

	/// <summary>
	/// 原名
	/// </summary>
	[Column(Name = "original_name", StringLength = 255, IsNullable = false)]
	public string OriginalName { get; set; }

	/// <summary>
	/// 文件后缀名（含前导点，如 .png）
	/// </summary>
	[Column(Name = "file_suffix", StringLength = 10, IsNullable = false)]
	public string FileSuffix { get; set; }

	/// <summary>
	/// URL地址
	/// </summary>
	[Column(Name = "url", StringLength = 500, IsNullable = false)]
	public string Url { get; set; }

	/// <summary>
	/// 扩展字段（存 SysOssExt 的 JSON 串）
	/// </summary>
	[Column(Name = "ext1", IsNullable = true)]
	public string Ext1 { get; set; }

	/// <summary>
	/// 服务商
	/// </summary>
	[Column(Name = "service", StringLength = 20, IsNullable = false)]
	public string Service { get; set; }
}
