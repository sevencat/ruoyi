using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 角色业务层（对应 Java 的 <c>SysRoleServiceImpl</c>）
/// </summary>
/// <remarks>
/// 目前只实现用户模块用到的部分。Java 中依赖数据权限的 <c>checkRoleDataScope</c> / <c>selectRoleCount</c>
/// 需要 <c>DataPermissionHelper</c> 支撑，C# 端暂无该设施，故未实现。
/// </remarks>
[Component]
public class SysRoleService(IFreeSql fsql, IMapper mapper)
{
	/// <summary>
	/// 根据用户ID查询角色列表（对应 Java 的 <c>selectRolesByUserId</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>角色列表</returns>
	public async Task<List<SysRoleVo>> SelectRolesByUserId(long userId)
	{
		// Java 的 selectRolesByUserId：sys_user_role 关联 sys_role，并按 r.del_flag = '0' 过滤
		var roleIds = await fsql.Select<TSysUserRole>()
			.Where(x => x.UserId == userId)
			.ToListAsync(x => x.RoleId);

		if (roleIds.Count == 0)
		{
			return [];
		}

		var roles = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => roleIds.Contains(x.RoleId))
			.OrderBy(x => x.RoleSort)
			.OrderBy(x => x.CreateTime)
			.ToListAsync();

		return roles.MapTo<List<SysRoleVo>>(mapper);
	}

	/// <summary>
	/// 查询全部角色列表（对应 Java 的 <c>selectRoleAll</c>）
	/// </summary>
	/// <returns>角色列表</returns>
	public async Task<List<SysRoleVo>> SelectRoleAll()
	{
		return await SelectRoleList(new SysRoleBo());
	}

	/// <summary>
	/// 根据条件查询角色列表（对应 Java 的 <c>selectRoleList</c>）
	/// </summary>
	/// <param name="role">角色筛选条件</param>
	/// <returns>角色列表</returns>
	public async Task<List<SysRoleVo>> SelectRoleList(SysRoleBo role)
	{
		var roles = await BuildRoleQuery(role).ToListAsync();
		return roles.MapTo<List<SysRoleVo>>(mapper);
	}

	/// <summary>
	/// 根据用户ID查询角色选择框列表（对应 Java 的 <c>selectRoleListByUserId</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>已选中的角色ID列表</returns>
	public async Task<List<long>> SelectRoleListByUserId(long userId)
	{
		// Java 的 selectRoleListByUserId 会按数据权限过滤（selectRoleCount / deptCheckStrictly），
		// 数据权限设施缺失时退化为返回该用户已有的全部角色ID
		var userRoles = await SelectRolesByUserId(userId);
		return userRoles.Select(x => x.RoleId).Where(x => x.HasValue).Select(x => x.Value).ToList();
	}

	/// <summary>
	/// 根据用户ID查询授权角色列表（对应 Java 的 <c>selectRolesAuthByUserId</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>全部角色列表，用户已拥有的角色 <c>Flag</c> 为 true</returns>
	public async Task<List<SysRoleVo>> SelectRolesAuthByUserId(long userId)
	{
		var userRoles = await SelectRolesByUserId(userId);
		var roles = await SelectRoleAll();

		// 使用 HashSet 提高查找效率
		var userRoleIds = userRoles.Select(x => x.RoleId).ToHashSet();
		foreach (var role in roles)
		{
			if (userRoleIds.Contains(role.RoleId))
			{
				role.Flag = true;
			}
		}

		return roles;
	}

	/// <summary>
	/// 构造角色列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="role">角色筛选条件</param>
	/// <returns>角色列表查询对象</returns>
	private ISelect<TSysRole> BuildRoleQuery(SysRoleBo role)
	{
		return fsql.Select<TSysRole>()
			.WhereNotNullEq(role.RoleId, x => x.RoleId)
			.WhereLike(role.RoleName, x => x.RoleName)
			.WhereHasTextEq(role.Status, x => x.Status)
			.WhereLike(role.RoleKey, x => x.RoleKey)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(role.Params, x => x.CreateTime)
			.OrderBy(x => x.RoleSort)
			.OrderBy(x => x.CreateTime);
	}
}
