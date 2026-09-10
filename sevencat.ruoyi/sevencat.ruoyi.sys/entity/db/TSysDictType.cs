using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.core.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 字典类型表
/// </summary>
[Table(Name = "sys_dict_type")]
[Index("dict_type", "DictType", true)]
public class TSysDictType : TBaseEntity
{
	/// <summary>
	/// 字典主键
	/// </summary>
	[Column(Name = "dict_id", IsPrimary = true)]
	[Snowflake]
	public long DictId { get; set; }

	/// <summary>
	/// 字典名称
	/// </summary>
	[Column(Name = "dict_name", StringLength = 100, IsNullable = true)]
	public string DictName { get; set; } = string.Empty;

	/// <summary>
	/// 字典类型
	/// </summary>
	[Column(Name = "dict_type", StringLength = 100, IsNullable = true)]
	public string DictType { get; set; } = string.Empty;

	/// <summary>
	/// 备注
	/// </summary>
	[Column(Name = "remark", StringLength = 500, IsNullable = true, Position = -1)]
	public string Remark { get; set; }
}
