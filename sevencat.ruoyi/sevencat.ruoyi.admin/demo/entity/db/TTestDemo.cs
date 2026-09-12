using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;

namespace sevencat.ruoyi.demo.entity.db;

/// <summary>
/// 测试单表 test_demo
/// </summary>
/// <remarks>
/// 对应 Java 的 <c>org.dromara.demo.domain.TestDemo</c>。
/// Java 原注解 <c>@TableLogic</c>（del_flag 逻辑删除）C# 端无插件支持，删除语句由 Service 手工构造；
/// Java 原注解 <c>@Version</c>（乐观锁）同理，version 由 Service 手工自增。
/// </remarks>
[Table(Name = "test_demo")]
// 对应 Java 的 TestDemoMapper 上 @DataPermission({ deptName -&gt; dept_id, userName -&gt; user_id })
[DataScope(DeptColumn = "DeptId", UserColumn = "UserId")]
public class TTestDemo : TBaseEntity
{
	/// <summary>
	/// 主键
	/// </summary>
	// Java 原注解 @TableId(value = "id")：MyBatis-Plus 默认雪花ID，C# 端用 [Snowflake] 等价
	[Column(Name = "id", IsPrimary = true)]
	[Snowflake]
	public long Id { get; set; }

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
	/// 排序号
	/// </summary>
	// Java 原注解 @OrderBy(asc = false, sort = 1)：默认按 order_num 倒序，排序在 Service 的查询中显式给出
	[Column(Name = "order_num", IsNullable = true)]
	public int? OrderNum { get; set; } = 0;

	/// <summary>
	/// key键
	/// </summary>
	[Column(Name = "test_key", StringLength = 255, IsNullable = true)]
	public string TestKey { get; set; }

	/// <summary>
	/// 值
	/// </summary>
	[Column(Name = "value", StringLength = 255, IsNullable = true)]
	public string Value { get; set; }

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
