namespace sevencat.ruoyi.common.db.attr;

/// <summary>
/// 实体数据权限列映射（对应 Java 的 <c>@DataPermission</c> / <c>@DataColumn</c>）。
/// </summary>
/// <remarks>
/// Java 中该注解标注在 Mapper 方法上，通过 key/value 指定 SQL 列名
/// （<c>deptName -&gt; dept_id / create_dept</c>、<c>userName -&gt; create_by</c>）；
/// C# 端查询在 Service 中内联构建、没有 Mapper，故改为标注在实体上并直接指定 C# 属性名。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DataScopeAttribute : Attribute
{
	/// <summary>
	/// 参与「部门」范围过滤的属性名（对应 Java 的 <c>deptName</c>），
	/// 为 null 表示该实体不参与部门条件（如 sys_dept 仅按自身 dept_id 过滤）
	/// </summary>
	public string DeptColumn { get; set; } = "DeptId";

	/// <summary>
	/// 参与「仅本人」范围过滤的属性名（对应 Java 的 <c>userName</c>，即 <c>create_by</c>），
	/// 为 null 表示该实体不参与本人条件
	/// </summary>
	public string UserColumn { get; set; } = "CreateBy";
}
