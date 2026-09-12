using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.demo.entity.db;

/// <summary>
/// 测试树表 test_tree
/// </summary>
/// <remarks>
/// 对应 Java 的 <c>org.dromara.demo.domain.TestTree</c>。
/// Java 原注解 <c>@TableLogic</c>（del_flag 逻辑删除）C# 端无插件支持，删除语句由 Service 手工构造；
/// Java 原注解 <c>@Version</c>（乐观锁）同理，version 由 Service 手工自增。
/// </remarks>
[Table(Name = "test_tree")]
// 对应 Java 的 TestTreeMapper 上 @DataPermission({ deptName -&gt; dept_id, userName -&gt; user_id })
[DataScope(DeptColumn = "DeptId", UserColumn = "UserId")]
public class TTestTree : TBaseEntity
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 原注解 @TableId(value = "id")：MyBatis-Plus 默认雪花ID，C# 端用 [Snowflake] 等价
	[Column(Name = "id", IsPrimary = true)]
	[Snowflake]
	public long Id { get; set; }

	/// <summary>
	/// 父ID
	/// </summary>
	[Column(Name = "parent_id", IsNullable = true)]
	public long? ParentId { get; set; } = 0;

	/// <summary>
	/// 部门id
	/// </summary>
	[Column(Name = "dept_id", IsNullable = true)]
	public long? DeptId { get; set; }

	/// <summary>
	/// 用户id
	/// </summary>
	[Column(Name = "user_id", IsNullable = true)]
	public long? UserId { get; set; }

	/// <summary>
	/// 树节点名
	/// </summary>
	[Column(Name = "tree_name", StringLength = 255, IsNullable = true)]
	public string TreeName { get; set; }

	/// <summary>
	/// 版本
	/// </summary>
	[Column(Name = "version", IsNullable = true)]
	public int? Version { get; set; } = 0;

	/// <summary>
	/// 删除标志（0代表存在 1代表删除）
	/// </summary>
	[Column(Name = "del_flag", IsNullable = true)]
	public int? DelFlag { get; set; } = 0;
}
