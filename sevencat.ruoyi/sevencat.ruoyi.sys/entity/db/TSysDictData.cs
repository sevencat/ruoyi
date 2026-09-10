using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.core.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 字典数据表
/// </summary>
[Table(Name = "sys_dict_data")]
[Index("idx_sys_dict_data_type", "DictType")]
public class TSysDictData : TBaseEntity
{
	/// <summary>
	/// 字典编码
	/// </summary>
	[Column(Name = "dict_code", IsPrimary = true)]
	[Snowflake]
	public long DictCode { get; set; }

	/// <summary>
	/// 字典排序
	/// </summary>
	[Column(Name = "dict_sort", IsNullable = true)]
	public int? DictSort { get; set; } = 0;

	/// <summary>
	/// 字典标签
	/// </summary>
	[Column(Name = "dict_label", StringLength = 100, IsNullable = true)]
	public string DictLabel { get; set; } = string.Empty;

	/// <summary>
	/// 字典键值
	/// </summary>
	[Column(Name = "dict_value", StringLength = 100, IsNullable = true)]
	public string DictValue { get; set; } = string.Empty;

	/// <summary>
	/// 字典类型
	/// </summary>
	[Column(Name = "dict_type", StringLength = 100, IsNullable = true)]
	public string DictType { get; set; } = string.Empty;

	/// <summary>
	/// 样式属性（其他样式扩展）
	/// </summary>
	[Column(Name = "css_class", StringLength = 100, IsNullable = true)]
	public string CssClass { get; set; }

	/// <summary>
	/// 表格回显样式
	/// </summary>
	[Column(Name = "list_class", StringLength = 100, IsNullable = true)]
	public string ListClass { get; set; }

	/// <summary>
	/// 是否默认（Y是 N否）
	/// </summary>
	[Column(Name = "is_default", StringLength = 1, IsNullable = true)]
	public string IsDefault { get; set; } = "N";

	/// <summary>
	/// 备注
	/// </summary>
	[Column(Name = "remark", StringLength = 500, IsNullable = true, Position = -1)]
	public string Remark { get; set; }
}
