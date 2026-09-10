using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.core.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 部门表
/// </summary>
[Table(Name = "sys_dept")]
[Index("idx_sys_dept_parent_id", "ParentId")]
public class TSysDept : TBaseEntity
{
	/// <summary>
	/// 部门id
	/// </summary>
	[Column(Name = "dept_id", IsPrimary = true)]
	[Snowflake]
	public long DeptId { get; set; }

	/// <summary>
	/// 父部门id
	/// </summary>
	[Column(Name = "parent_id", IsNullable = true)]
	public long? ParentId { get; set; } = 0;

	/// <summary>
	/// 祖级列表
	/// </summary>
	[Column(Name = "ancestors", StringLength = 500, IsNullable = true)]
	public string Ancestors { get; set; } = string.Empty;

	/// <summary>
	/// 部门名称
	/// </summary>
	[Column(Name = "dept_name", StringLength = 30, IsNullable = true)]
	public string DeptName { get; set; } = string.Empty;

	/// <summary>
	/// 部门类别编码
	/// </summary>
	[Column(Name = "dept_category", StringLength = 100, IsNullable = true)]
	public string DeptCategory { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	[Column(Name = "order_num", IsNullable = true)]
	public int? OrderNum { get; set; } = 0;

	/// <summary>
	/// 负责人
	/// </summary>
	[Column(Name = "leader", IsNullable = true)]
	public long? Leader { get; set; }

	/// <summary>
	/// 联系电话
	/// </summary>
	[Column(Name = "phone", StringLength = 11, IsNullable = true)]
	public string Phone { get; set; }

	/// <summary>
	/// 邮箱
	/// </summary>
	[Column(Name = "email", StringLength = 50, IsNullable = true)]
	public string Email { get; set; }

	/// <summary>
	/// 部门状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; } = "0";

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", StringLength = 1, IsNullable = true)]
	public string DelFlag { get; set; } = "0";
}
