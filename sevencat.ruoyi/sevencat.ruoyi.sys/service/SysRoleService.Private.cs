using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 角色业务层 —— 查询条件构造与关联表写入等私有辅助实现
/// </summary>
/// <remarks>
/// 对外接口与主流程见 <c>SysRoleService.cs</c>，本文件只承载不对外暴露的辅助逻辑。
/// </remarks>
public partial class SysRoleService
{
	/// <summary>
	/// 构造角色列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="role">角色筛选条件</param>
	/// <returns>角色列表查询对象</returns>
	private ISelect<TSysRole> BuildRoleQuery(SysRoleBo role)
	{
		return fsql.Select<TSysRole>()
			// 对应 Java 的 @TableLogic：只查询未删除的角色
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereNotNullEq(role.RoleId, x => x.RoleId)
			.WhereLike(role.RoleName, x => x.RoleName)
			.WhereHasTextEq(role.Status, x => x.Status)
			.WhereLike(role.RoleKey, x => x.RoleKey)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(role.Params, x => x.CreateTime)
			.OrderBy(x => x.RoleSort)
			.OrderBy(x => x.CreateTime);
	}

	/// <summary>
	/// 新增角色和菜单关联（对应 Java 的 <c>insertRoleMenu</c>）
	/// </summary>
	/// <param name="role">角色信息（含菜单ID集合）</param>
	/// <returns>影响行数</returns>
	private int InsertRoleMenu(SysRoleBo role)
	{
		var rows = 1;
		if (role.MenuIds is { Length: > 0 } && role.RoleId.HasValue)
		{
			var list = role.MenuIds
				.Select(menuId => new TSysRoleMenu { RoleId = role.RoleId.Value, MenuId = menuId })
				.ToList();

			var affrows = fsql.Insert(list).ExecuteAffrows();
			rows = affrows > 0 ? list.Count : 0;
		}

		return rows;
	}

	/// <summary>
	/// 新增角色和部门关联（对应 Java 的 <c>insertRoleDept</c>）
	/// </summary>
	/// <param name="role">角色信息（含部门ID集合）</param>
	/// <returns>影响行数</returns>
	private int InsertRoleDept(SysRoleBo role)
	{
		var rows = 1;
		if (role.DeptIds is { Length: > 0 } && role.RoleId.HasValue)
		{
			var list = role.DeptIds
				.Select(deptId => new TSysRoleDept { RoleId = role.RoleId.Value, DeptId = deptId })
				.ToList();

			var affrows = fsql.Insert(list).ExecuteAffrows();
			rows = affrows > 0 ? list.Count : 0;
		}

		return rows;
	}
}
