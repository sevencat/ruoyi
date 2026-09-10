using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.core.entity.db;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 岗位信息表
/// </summary>
[Table(Name = "sys_post")]
[Index("idx_sys_post_dept_id", "DeptId")]
public class TSysPost : TBaseEntity
{
	/// <summary>
	/// 岗位ID
	/// </summary>
	[Column(Name = "post_id", IsPrimary = true)]
	[Snowflake]
	public long PostId { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	[Column(Name = "dept_id", IsNullable = false)]
	public long DeptId { get; set; }

	/// <summary>
	/// 岗位编码
	/// </summary>
	[Column(Name = "post_code", StringLength = 64, IsNullable = false)]
	public string PostCode { get; set; }

	/// <summary>
	/// 岗位类别编码
	/// </summary>
	[Column(Name = "post_category", StringLength = 100, IsNullable = true)]
	public string PostCategory { get; set; }

	/// <summary>
	/// 岗位名称
	/// </summary>
	[Column(Name = "post_name", StringLength = 50, IsNullable = false)]
	public string PostName { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	[Column(Name = "post_sort", IsNullable = false)]
	public int PostSort { get; set; }

	/// <summary>
	/// 状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = false)]
	public string Status { get; set; }

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", StringLength = 1, IsNullable = true)]
	public string DelFlag { get; set; } = "0";
}
