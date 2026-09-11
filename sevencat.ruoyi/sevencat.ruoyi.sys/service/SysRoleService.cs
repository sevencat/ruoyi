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
/// 角色业务层（对应 Java 的 <c>SysRoleServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 中依赖数据权限的 <c>checkRoleDataScope</c> / <c>selectRoleCount</c> 需要
/// <c>DataPermissionHelper</c> 支撑，C# 端暂无该设施，对应实现已退化为「主键存在性校验」。
/// </remarks>
[Component]
public class SysRoleService(IFreeSql fsql, IMapper mapper, LoginService loginService)
{
	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const string DEL_FLAG_DELETED = "1";

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
	/// 分页查询角色列表（对应 Java 的 <c>selectPageRoleList</c>）
	/// </summary>
	/// <param name="role">角色筛选条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>角色分页结果</returns>
	public async Task<PageResult<SysRoleVo>> SelectPageRoleList(SysRoleBo role, PageQuery2 pageQuery)
	{
		return (await BuildRoleQuery(role).ToPage(pageQuery)).MapTo<SysRoleVo>(mapper);
	}

	/// <summary>
	/// 根据角色ID查询角色（对应 Java 的 <c>selectRoleById</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>角色详情</returns>
	public async Task<SysRoleVo> SelectRoleById(long roleId)
	{
		var role = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.RoleId == roleId)
			.FirstAsync();

		return role?.MapTo<SysRoleVo>(mapper);
	}

	/// <summary>
	/// 根据角色ID查询角色列表（对应 Java 的 <c>selectRoleByIds</c>）
	/// </summary>
	/// <param name="roleIds">角色ID列表</param>
	/// <returns>角色列表</returns>
	public async Task<List<SysRoleVo>> SelectRoleByIds(List<long> roleIds)
	{
		// 对应 Java 的 selectRoleList(lambda().eq(status, NORMAL).inIfNotEmpty(roleId, roleIds))
		var query = fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereHasTextEq(SystemConstants.NORMAL, x => x.Status);

		if (roleIds is { Count: > 0 })
		{
			query = query.Where(x => roleIds.Contains(x.RoleId));
		}

		var roles = await query.ToListAsync();
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
	/// 校验角色名称是否唯一（对应 Java 的 <c>checkRoleNameUnique</c>）
	/// </summary>
	/// <param name="role">角色信息</param>
	/// <returns>唯一返回 true</returns>
	public async Task<bool> CheckRoleNameUnique(SysRoleBo role)
	{
		// Java 的 lambda() 会自动追加 @TableLogic 的 del_flag = '0'
		var exist = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.RoleName == role.RoleName)
			.WhereIf(role.RoleId.HasValue, x => x.RoleId != role.RoleId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验角色权限标识是否唯一（对应 Java 的 <c>checkRoleKeyUnique</c>）
	/// </summary>
	/// <param name="role">角色信息</param>
	/// <returns>唯一返回 true</returns>
	public async Task<bool> CheckRoleKeyUnique(SysRoleBo role)
	{
		var exist = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.RoleKey == role.RoleKey)
			.WhereIf(role.RoleId.HasValue, x => x.RoleId != role.RoleId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验角色是否允许操作（对应 Java 的 <c>checkRoleAllowed</c>）
	/// </summary>
	/// <param name="role">角色信息</param>
	public async Task CheckRoleAllowed(SysRoleBo role)
	{
		if (role.RoleId.HasValue && role.RoleId.Value == SystemConstants.SUPER_ADMIN_ROLE_ID)
		{
			throw new ServiceException("不允许操作超级管理员角色");
		}

		// 新增不允许使用 管理员标识符
		if (!role.RoleId.HasValue && SystemConstants.SUPER_ADMIN_ROLE_KEY.Equals(role.RoleKey))
		{
			throw new ServiceException("不允许使用系统内置管理员角色标识符!");
		}

		// 修改不允许修改 管理员标识符
		if (role.RoleId.HasValue)
		{
			var sysRole = await fsql.Select<TSysRole>().Where(x => x.RoleId == role.RoleId.Value).FirstAsync();
			if (sysRole == null)
			{
				return;
			}

			// 标识符不相等说明修改了管理员标识符
			if (!string.Equals(sysRole.RoleKey, role.RoleKey, StringComparison.Ordinal))
			{
				if (SystemConstants.SUPER_ADMIN_ROLE_KEY.Equals(sysRole.RoleKey))
				{
					throw new ServiceException("不允许修改系统内置管理员角色标识符!");
				}

				if (SystemConstants.SUPER_ADMIN_ROLE_KEY.Equals(role.RoleKey))
				{
					throw new ServiceException("不允许使用系统内置管理员角色标识符!");
				}
			}
		}
	}

	/// <summary>
	/// 校验角色数据权限（对应 Java 的 <c>checkRoleDataScope(Long roleId)</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	public async Task CheckRoleDataScope(long? roleId)
	{
		if (!roleId.HasValue)
		{
			return;
		}

		await CheckRoleDataScope([roleId.Value]);
	}

	/// <summary>
	/// 批量校验角色数据权限（对应 Java 的 <c>checkRoleDataScope(Collection&lt;Long&gt; roleIds)</c>）
	/// </summary>
	/// <param name="roleIds">角色ID列表</param>
	public async Task CheckRoleDataScope(List<long> roleIds)
	{
		if (roleIds is not { Count: > 0 })
		{
			return;
		}

		var loginUser = await loginService.GetLoginUser();
		if (loginUser != null && loginUser.IsSuperAdmin())
		{
			return;
		}

		// Java 通过 @DataPermission 的 selectRoleCount 套数据权限；C# 端暂无该设施，退化为主键存在性校验
		var count = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => roleIds.Contains(x.RoleId))
			.CountAsync();

		if (count != roleIds.Count)
		{
			throw new ServiceException("没有权限访问部分角色数据！");
		}
	}

	/// <summary>
	/// 统计角色已分配的用户数量（对应 Java 的 <c>countUserRoleByRoleId</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>分配数量</returns>
	public async Task<long> CountUserRoleByRoleId(long? roleId)
	{
		return await fsql.Select<TSysUserRole>().Where(x => x.RoleId == roleId).CountAsync();
	}

	/// <summary>
	/// 新增角色（对应 Java 的 <c>insertRole</c>）
	/// </summary>
	/// <param name="bo">角色信息（含菜单ID集合）</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @Transactional(rollbackFor = Exception.class)：C# 端未引入事务包装，多次写库未置于同一事务
	public async Task<int> InsertRole(SysRoleBo bo)
	{
		var role = bo.MapTo<TSysRole>(mapper);
		role.CreateBy ??= await loginService.GetLoginuid();
		await fsql.Insert(role).ExecuteAffrowsAsync();
		// 回填雪花ID，供 insertRoleMenu / insertRoleDept 使用
		bo.RoleId = role.RoleId;

		return await InsertRoleMenu(bo);
	}

	/// <summary>
	/// 修改角色基本信息（对应 Java 的 <c>updateRoleBaseInfo</c>）
	/// </summary>
	/// <param name="bo">角色信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateRoleBaseInfo(SysRoleBo bo)
	{
		var role = bo.MapTo<TSysRole>(mapper);
		if (SystemConstants.DISABLE.Equals(role.Status) && await CountUserRoleByRoleId(role.RoleId) > 0)
		{
			throw new ServiceException("角色已分配，不能禁用!");
		}

		role.UpdateBy ??= await loginService.GetLoginuid();
		return await fsql.Update<TSysRole>().SetSourceIgnore(role).Where(a => a.RoleId == role.RoleId).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 修改角色数据权限（对应 Java 的 <c>updateRolePermission</c>）
	/// </summary>
	/// <param name="bo">角色信息（含菜单ID、部门ID集合）</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @CacheEvict(cacheNames = CacheNames.SYS_ROLE_CUSTOM, key = "#bo.roleId")：C# 端无角色自定义数据权限缓存，未实现
	// Java 原注解 @Transactional(rollbackFor = Exception.class)：C# 端未引入事务包装，多次写库未置于同一事务
	public async Task<int> UpdateRolePermission(SysRoleBo bo)
	{
		var role = bo.MapTo<TSysRole>(mapper);
		role.UpdateBy ??= await loginService.GetLoginuid();
		await fsql.Update<TSysRole>().SetSourceIgnore(role).Where(a => a.RoleId == role.RoleId).ExecuteAffrowsAsync();

		await fsql.Delete<TSysRoleMenu>().Where(x => x.RoleId == role.RoleId).ExecuteAffrowsAsync();
		await InsertRoleMenu(bo);

		await fsql.Delete<TSysRoleDept>().Where(x => x.RoleId == role.RoleId).ExecuteAffrowsAsync();
		return await InsertRoleDept(bo);
	}

	/// <summary>
	/// 修改角色状态（对应 Java 的 <c>updateRoleStatus</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <param name="status">角色状态</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateRoleStatus(long? roleId, string status)
	{
		if (SystemConstants.DISABLE.Equals(status) && await CountUserRoleByRoleId(roleId) > 0)
		{
			throw new ServiceException("角色已分配，不能禁用!");
		}

		if (!roleId.HasValue)
		{
			return 0;
		}

		var update = fsql.Update<TSysRole>().Set(x => x.Status, status);
		// C# 端约定：状态变更同步维护更新人与更新时间
		var updateBy = await loginService.GetLoginuid();
		if (updateBy.HasValue)
		{
			update = update.Set(x => x.UpdateBy, updateBy.Value);
		}

		return await update
			.Set(x => x.UpdateTime, DateTime.Now)
			.Where(x => x.RoleId == roleId.Value)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 批量删除角色（对应 Java 的 <c>deleteRoleByIds</c>）
	/// </summary>
	/// <param name="roleIds">角色ID列表</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @CacheEvict(cacheNames = CacheNames.SYS_ROLE_CUSTOM, allEntries = true)：C# 端无角色自定义数据权限缓存，未实现
	// Java 原注解 @Transactional(rollbackFor = Exception.class)：C# 端未引入事务包装，多次写库未置于同一事务
	public async Task<int> DeleteRoleByIds(List<long> roleIds)
	{
		await CheckRoleDataScope(roleIds);

		var roles = await fsql.Select<TSysRole>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => roleIds.Contains(x.RoleId))
			.ToListAsync();

		foreach (var role in roles)
		{
			await CheckRoleAllowed(role.MapTo<SysRoleBo>(mapper));

			if (await CountUserRoleByRoleId(role.RoleId) > 0)
			{
				throw new ServiceException($"{role.RoleName}已分配，不能删除!");
			}

			// Java 通过 OnlineUserCleanEvent.byRole(role.getRoleId()) 踢出在线用户；C# 端无事件总线，未实现
		}

		await fsql.Delete<TSysRoleMenu>().Where(x => roleIds.Contains(x.RoleId)).ExecuteAffrowsAsync();
		await fsql.Delete<TSysRoleDept>().Where(x => roleIds.Contains(x.RoleId)).ExecuteAffrowsAsync();

		// 对应 Java 实体上的 @TableLogic：udpate sys_role set del_flag = '1'
		return await fsql.Update<TSysRole>()
			.Set(x => x.DelFlag, DEL_FLAG_DELETED)
			.Where(x => roleIds.Contains(x.RoleId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 取消用户授权（对应 Java 的 <c>deleteAuthUser</c>）
	/// </summary>
	/// <param name="userRole">用户与角色关联</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteAuthUser(SysUserRoleBo userRole)
	{
		// 校验当前用户不允许操作自己
		var loginUid = await loginService.GetLoginuid();
		if (loginUid.HasValue && loginUid.Value == userRole.UserId)
		{
			throw new ServiceException("不允许修改当前用户角色!");
		}

		var rows = await fsql.Delete<TSysUserRole>()
			.Where(x => x.RoleId == userRole.RoleId)
			.Where(x => x.UserId == userRole.UserId)
			.ExecuteAffrowsAsync();

		// Java 通过 OnlineUserCleanEvent.byUserIds(userRole.getUserId()) 踢出在线用户；C# 端无事件总线，未实现
		return rows;
	}

	/// <summary>
	/// 批量取消用户授权（对应 Java 的 <c>deleteAuthUsers</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <param name="userIds">用户ID列表</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteAuthUsers(long? roleId, List<long> userIds)
	{
		if (userIds is not { Count: > 0 } || !roleId.HasValue)
		{
			return 0;
		}

		var loginUid = await loginService.GetLoginuid();
		if (loginUid.HasValue && userIds.Contains(loginUid.Value))
		{
			throw new ServiceException("不允许修改当前用户角色!");
		}

		var rows = await fsql.Delete<TSysUserRole>()
			.Where(x => x.RoleId == roleId.Value)
			.Where(x => userIds.Contains(x.UserId))
			.ExecuteAffrowsAsync();

		// Java 通过 OnlineUserCleanEvent.byUserIds(userIds) 踢出在线用户；C# 端无事件总线，未实现
		return rows;
	}

	/// <summary>
	/// 批量授权用户（对应 Java 的 <c>insertAuthUsers</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <param name="userIds">用户ID列表</param>
	/// <returns>影响行数</returns>
	public async Task<int> InsertAuthUsers(long roleId, List<long> userIds)
	{
		var loginUid = await loginService.GetLoginuid();
		if (loginUid.HasValue && userIds.Contains(loginUid.Value))
		{
			throw new ServiceException("不允许修改当前用户角色!");
		}

		var rows = 1;
		if (userIds is { Count: > 0 })
		{
			var list = userIds.Select(userId => new TSysUserRole { UserId = userId, RoleId = roleId }).ToList();
			var affrows = await fsql.Insert(list).ExecuteAffrowsAsync();
			rows = affrows > 0 ? list.Count : 0;
		}

		return rows;
	}

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
	private async Task<int> InsertRoleMenu(SysRoleBo role)
	{
		var rows = 1;
		if (role.MenuIds is { Length: > 0 } && role.RoleId.HasValue)
		{
			var list = role.MenuIds
				.Select(menuId => new TSysRoleMenu { RoleId = role.RoleId.Value, MenuId = menuId })
				.ToList();

			var affrows = await fsql.Insert(list).ExecuteAffrowsAsync();
			rows = affrows > 0 ? list.Count : 0;
		}

		return rows;
	}

	/// <summary>
	/// 新增角色和部门关联（对应 Java 的 <c>insertRoleDept</c>）
	/// </summary>
	/// <param name="role">角色信息（含部门ID集合）</param>
	/// <returns>影响行数</returns>
	private async Task<int> InsertRoleDept(SysRoleBo role)
	{
		var rows = 1;
		if (role.DeptIds is { Length: > 0 } && role.RoleId.HasValue)
		{
			var list = role.DeptIds
				.Select(deptId => new TSysRoleDept { RoleId = role.RoleId.Value, DeptId = deptId })
				.ToList();

			var affrows = await fsql.Insert(list).ExecuteAffrowsAsync();
			rows = affrows > 0 ? list.Count : 0;
		}

		return rows;
	}
}
