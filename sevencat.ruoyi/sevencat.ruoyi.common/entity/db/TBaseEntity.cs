using FreeSql.DataAnnotations;

namespace sevencat.ruoyi.common.entity.db;

public class TBaseEntity
{
	[Column(Name = "create_dept", IsNullable = true, Position = -5)]
	public long? CreateDept { get; set; }

	[Column(Name = "create_by", IsNullable = true, Position = -5)]
	public long? CreateBy { get; set; }

	[Column(Name = "create_time", IsNullable = true, Position = -4, CanUpdate = false)]
	public DateTime? CreateTime { get; set; }

	[Column(Name = "update_by", IsNullable = true, Position = -3)]
	public long? UpdateBy { get; set; }

	[Column(Name = "update_time", IsNullable = true, Position = -2)]
	public DateTime? UpdateTime { get; set; }

	// [Column(Name = "remark", IsNullable = true, StringLength = 500,Position = -1)]
	// public string Remark { get; set; }
}