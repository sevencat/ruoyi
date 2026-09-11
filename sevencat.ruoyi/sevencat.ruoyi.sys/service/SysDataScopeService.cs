using Autofac.Annotation;
using FreeSql;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.datapermission;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 系统数据权限服务（对应 Java 的 <c>SysDataScopeServiceImpl</c>，即 SpEL 模板中的 <c>@sdss</c>）。
/// </summary>
/// <remarks>
/// 与 <see cref="SysDeptService.SelectDeptAndChildById"/> 逻辑保持一致，
/// 但此处必须提供<b>同步</b>实现：数据权限是在 Freesql 解析表达式（同步 AOP）过程中调用的。
/// </remarks>
[Component]
public class SysDataScopeService(IFreeSql fsql) : IDataScopeService
{
	/// <summary>
	/// 获取角色已分配的自定义部门ID集合（对应 Java 的 <c>getRoleCustom</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>部门ID集合</returns>
	public List<long> GetRoleCustom(long roleId)
	{
		return fsql.Select<TSysRoleDept>()
			.Where(x => x.RoleId == roleId)
			.ToList(x => x.DeptId);
	}

	/// <summary>
	/// 获取指定部门及其所有子部门ID集合（对应 Java 的 <c>getDeptAndChild</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门ID集合</returns>
	public List<long> GetDeptAndChild(long deptId)
	{
		var depts = fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToList();

		return depts
			.Where(dept => IsDeptOrChild(dept, deptId))
			.Select(dept => dept.DeptId)
			.ToList();
	}

	/// <summary>
	/// 判断部门是否为指定部门或其子部门（对应 Java 的 <c>find_in_set(deptId, ancestors)</c> 判定）
	/// </summary>
	/// <param name="dept">部门</param>
	/// <param name="deptId">目标部门ID</param>
	/// <returns>true 是 false 否</returns>
	private static bool IsDeptOrChild(TSysDept dept, long deptId)
	{
		return dept.DeptId == deptId || $"{dept.Ancestors},".Contains($"{deptId},");
	}
}
