namespace sevencat.ruoyi.common.db.datapermission;

/// <summary>
/// 数据权限范围类型（对应 Java 的 <c>org.dromara.common.mybatis.enums.DataScopeType</c>）。
/// </summary>
/// <remarks>
/// Java 通过 SpEL 模板拼接 SQL 片段（模板见每个枚举值注释）；
/// C# 端不使用 SpEL，改为在 Freesql AOP 中构建等价的表达式树条件。
/// </remarks>
public enum DataScopeType
{
	/// <summary>
	/// 全部数据权限（Java 模板为空，即不追加任何条件）
	/// </summary>
	All = 1,

	/// <summary>
	/// 自定义数据权限（Java 模板：<c>#{#deptName} IN (#{@sdss.getRoleCustom(#roleId)})</c>）
	/// </summary>
	Custom = 2,

	/// <summary>
	/// 本部门数据权限（Java 模板：<c>#{#deptName} = #{#user.deptId}</c>）
	/// </summary>
	Dept = 3,

	/// <summary>
	/// 本部门及以下数据权限（Java 模板：<c>#{#deptName} IN (#{@sdss.getDeptAndChild(#user.deptId)})</c>）
	/// </summary>
	DeptAndChild = 4,

	/// <summary>
	/// 仅本人数据权限（Java 模板：<c>#{#userName} = #{#user.userId}</c>）
	/// </summary>
	Self = 5,

	/// <summary>
	/// 本部门及以下或本人数据权限（Java 模板：部门及以下 OR 本人）
	/// </summary>
	DeptAndChildOrSelf = 6,
}

/// <summary>
/// <see cref="DataScopeType"/> 辅助方法
/// </summary>
public static class DataScopeTypeHelper
{
	/// <summary>
	/// 根据范围编码查找枚举（对应 Java 的 <c>DataScopeType.findCode</c>）
	/// </summary>
	/// <param name="code">范围编码（1-6）</param>
	/// <returns>匹配的枚举；未找到时返回 null</returns>
	public static DataScopeType? FindCode(string code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return null;
		}

		return int.TryParse(code, out var value) && Enum.IsDefined(typeof(DataScopeType), value)
			? (DataScopeType)value
			: null;
	}
}
