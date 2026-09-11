namespace sevencat.ruoyi.common.db.datapermission;

/// <summary>
/// 系统数据权限服务（对应 Java 的 <c>ISysDataScopeService</c>，即 SpEL 模板中的 <c>@sdss</c>）。
/// </summary>
/// <remarks>
/// 实现位于系统模块（<c>SysDataScopeService</c>），通过 <c>IocFactory</c> 延迟解析，
/// 以避免 common 层反向依赖 sys 层的实体。
/// </remarks>
public interface IDataScopeService
{
	/// <summary>
	/// 获取角色已分配的自定义部门ID集合（对应 Java 的 <c>getRoleCustom</c>，数据来源 sys_role_dept）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>部门ID集合</returns>
	List<long> GetRoleCustom(long roleId);

	/// <summary>
	/// 获取指定部门及其所有子部门ID集合（对应 Java 的 <c>getDeptAndChild</c>，数据来源 sys_dept.ancestors）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门ID集合</returns>
	List<long> GetDeptAndChild(long deptId);
}
