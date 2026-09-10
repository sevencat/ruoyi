namespace sevencat.ruoyi.sys.dto;

/// <summary>
/// 部门
/// </summary>
public class DeptDTO
{
	/// <summary>
	/// 部门ID
	/// </summary>
	public long? DeptId { get; set; }

	/// <summary>
	/// 父部门ID
	/// </summary>
	public long? ParentId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	public string DeptName { get; set; }
}
