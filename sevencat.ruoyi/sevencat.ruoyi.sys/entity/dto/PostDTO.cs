namespace sevencat.ruoyi.sys.entity.dto;

/// <summary>
/// 岗位简要信息对象
/// </summary>
public class PostDTO
{
	/// <summary>
	/// 岗位ID
	/// </summary>
	public long? PostId { get; set; }

	/// <summary>
	/// 部门id
	/// </summary>
	public long? DeptId { get; set; }

	/// <summary>
	/// 岗位编码
	/// </summary>
	public string PostCode { get; set; }

	/// <summary>
	/// 岗位名称
	/// </summary>
	public string PostName { get; set; }

	/// <summary>
	/// 岗位类别编码
	/// </summary>
	public string PostCategory { get; set; }
}
