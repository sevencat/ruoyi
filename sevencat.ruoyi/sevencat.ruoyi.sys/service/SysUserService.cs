using System.Text;
using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.excel;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 用户信息业务层（对应 Java 的 <c>SysUserServiceImpl</c>）
/// </summary>
[Component]
public class SysUserService(
	IFreeSql fsql,
	IMapper mapper,
	LoginService loginService,
	SysDeptService deptService,
	SysRoleService roleService,
	SysPostService postService,
	SysConfigService configService,
	ExcelDictFormatConverter dictFormatConverter)
{
	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const string DEL_FLAG_DELETED = "1";

	/// <summary>
	/// 分页查询用户列表（对应 Java 的 <c>selectPageUserList</c>）
	/// </summary>
	/// <param name="user">用户查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页列表（已回填部门名称）</returns>
	public async Task<PageResult<SysUserVo>> SelectPageUserList(SysUserBo user, PageQuery2 pageQuery)
	{
		var result = (await BuildUserQuery(user).ToPage(pageQuery)).MapTo<SysUserVo>(mapper);
		await FillDeptName(result.Rows);
		return result;
	}

	/// <summary>
	/// 根据条件查询用户导出列表（对应 Java 的 <c>selectUserExportList</c>）
	/// </summary>
	/// <param name="user">用户查询条件</param>
	/// <returns>用户导出列表</returns>
	public async Task<List<SysUserExportVo>> SelectUserExportList(SysUserBo user)
	{
		// 对应 Java：deptId 不为空时先取出部门及其所有子部门ID
		List<long> deptIds = null;
		if (user.DeptId.HasValue)
		{
			deptIds = await deptService.SelectDeptAndChildById(user.DeptId.Value);
		}

		// 对应 Java 的 selectUserExportList 自定义 SQL：注意其条件比列表查询少，
		// 只有 userName / nickName / status / phoneNumber / createTime / deptIds（del_flag 由 @TableLogic 隐式追加）
		var q = fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereLike(user.UserName, x => x.UserName)
			.WhereLike(user.NickName, x => x.NickName)
			.WhereHasTextEq(user.Status, x => x.Status)
			.WhereLike(user.PhoneNumber, x => x.PhoneNumber)
			.WhereTimeRange(user.Params, x => x.CreateTime);

		if (deptIds is { Count: > 0 })
		{
			var ids = deptIds.Select(id => (long?)id).ToList();
			q = q.Where(x => ids.Contains(x.DeptId));
		}

		var users = await q.OrderBy(x => x.UserId).ToListAsync();
		var result = users.MapTo<List<SysUserExportVo>>(mapper);

		// 部门名称：对应 Java 的 DeptExcelConverter，按部门ID转换为「父级/子级」全路径名称
		var deptNames = await deptService.SelectDeptPathNames();
		for (var i = 0; i < users.Count; i++)
		{
			if (users[i].DeptId.HasValue && deptNames.TryGetValue(users[i].DeptId.Value, out var deptName))
			{
				result[i].DeptName = deptName;
			}
		}

		// 部门负责人：对应 Java 的 left join sys_user u1 on u1.user_id = d.leader
		await FillLeaderName(users, result);

		// 字典转换：用户性别 / 账号状态（对应 Java 的 @ExcelDictFormat 转换器）
		await dictFormatConverter.ToExcelData(result);
		return result;
	}

	/// <summary>
	/// 根据用户ID查询用户信息（对应 Java 的 <c>selectUserById</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>用户信息（含角色列表），不存在时返回 null</returns>
	// Java 原注解 @Cacheable(cacheNames = CacheNames.SYS_USER_NAME)：C# 端暂无该缓存，未实现
	public async Task<SysUserVo> SelectUserById(long userId)
	{
		var user = await fsql.Select<TSysUser>().Where(x => x.UserId == userId).ToOneAsync();
		if (user == null)
		{
			return null;
		}

		var vo = user.MapTo<SysUserVo>(mapper);
		vo.Roles = await roleService.SelectRolesByUserId(userId);
		return vo;
	}

	/// <summary>
	/// 通过用户账号查询用户信息（对应 Java 的 <c>selectUserByUserName</c>）
	/// </summary>
	/// <param name="userName">用户账号</param>
	/// <returns>用户信息（含角色列表），不存在时返回 null</returns>
	// Java 原注解 @Cacheable(cacheNames = CacheNames.SYS_USER_NAME)：C# 端暂无该缓存，未实现
	public async Task<SysUserVo> SelectUserByUserName(string userName)
	{
		var user = await fsql.Select<TSysUser>().Where(x => x.UserName == userName).ToOneAsync();
		if (user == null)
		{
			return null;
		}

		var vo = user.MapTo<SysUserVo>(mapper);
		vo.Roles = await roleService.SelectRolesByUserId(user.UserId);
		return vo;
	}

	/// <summary>
	/// 根据用户ID列表和部门ID查询用户基础信息（对应 Java 的 <c>selectUserByIds</c>）
	/// </summary>
	/// <param name="userIds">用户ID列表，为空时不过滤</param>
	/// <param name="deptId">部门ID，为空时不过滤</param>
	/// <returns>用户基础信息列表</returns>
	public async Task<List<SysUserVo>> SelectUserByIds(List<long> userIds, long? deptId)
	{
		// 对应 Java 的 selectUserByIds：status = '0'，userIds / deptId 判空匹配，按 user_id 排序
		var q = fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereHasTextEq(SystemConstants.NORMAL, x => x.Status)
			.WhereNotNullEq(deptId, x => x.DeptId);

		if (userIds is { Count: > 0 })
		{
			q = q.Where(x => userIds.Contains(x.UserId));
		}

		var users = await q.OrderBy(x => x.UserId).ToListAsync();
		return users.MapTo<List<SysUserVo>>(mapper);
	}

	/// <summary>
	/// 查询指定部门下的全部用户（对应 Java 的 <c>selectUserListByDept</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>用户列表</returns>
	public async Task<List<SysUserVo>> SelectUserListByDept(long deptId)
	{
		var users = await fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.DeptId == deptId)
			.ToListAsync();

		return users.MapTo<List<SysUserVo>>(mapper);
	}

	/// <summary>
	/// 查询角色已分配的用户列表（对应 Java 的 <c>selectAllocatedList</c>）
	/// </summary>
	/// <param name="user">用户查询条件（需包含 roleId）</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页列表（已回填部门名称）</returns>
	public async Task<PageResult<SysUserVo>> SelectAllocatedList(SysUserBo user, PageQuery2 pageQuery)
	{
		var q = BuildUserRoleJoinQuery(user);

		// 对应 Java 的 eq("r", SysRole::getRoleId, user.getRoleId())
		if (user.RoleId.HasValue)
		{
			var roleUserIds = await SelectUserIdsByRoleId(user.RoleId);
			q = roleUserIds.Count == 0 ? q.Where(x => false) : q.Where(x => roleUserIds.Contains(x.UserId));
		}

		var result = (await q.ToPage(pageQuery)).MapTo<SysUserVo>(mapper);
		await FillDeptName(result.Rows);
		return result;
	}

	/// <summary>
	/// 查询角色未分配的用户列表（对应 Java 的 <c>selectUnallocatedList</c>）
	/// </summary>
	/// <param name="user">用户查询条件（需包含 roleId）</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>用户分页列表（已回填部门名称）</returns>
	public async Task<PageResult<SysUserVo>> SelectUnallocatedList(SysUserBo user, PageQuery2 pageQuery)
	{
		var roleUserIds = await SelectUserIdsByRoleId(user.RoleId);
		var q = BuildUserRoleJoinQuery(user);

		// 对应 Java 的 notIn(SysUser::getUserId, userIds)
		if (roleUserIds.Count > 0)
		{
			q = q.Where(x => !roleUserIds.Contains(x.UserId));
		}

		var result = (await q.ToPage(pageQuery)).MapTo<SysUserVo>(mapper);
		await FillDeptName(result.Rows);
		return result;
	}

	/// <summary>
	/// 查询角色已关联的用户ID列表（对应 Java 的 <c>SysUserMapper.selectUserIdsByRoleId</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>用户ID列表</returns>
	public async Task<List<long>> SelectUserIdsByRoleId(long? roleId)
	{
		if (!roleId.HasValue)
		{
			return [];
		}

		return await fsql.Select<TSysUserRole>()
			.Where(r => r.RoleId == roleId.Value)
			.ToListAsync(r => r.UserId);
	}

	/// <summary>
	/// 校验用户账号是否唯一（对应 Java 的 <c>checkUserNameUnique</c>）
	/// </summary>
	/// <param name="user">用户信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckUserNameUnique(SysUserBo user)
	{
		var exist = await fsql.Select<TSysUser>()
			.Where(x => x.UserName == user.UserName)
			.WhereIf(user.UserId.HasValue, x => x.UserId != user.UserId)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验手机号码是否唯一（对应 Java 的 <c>checkPhoneUnique</c>）
	/// </summary>
	/// <param name="user">用户信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckPhoneUnique(SysUserBo user)
	{
		var exist = await fsql.Select<TSysUser>()
			.Where(x => x.PhoneNumber == user.PhoneNumber)
			.WhereIf(user.UserId.HasValue, x => x.UserId != user.UserId)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验邮箱是否唯一（对应 Java 的 <c>checkEmailUnique</c>）
	/// </summary>
	/// <param name="user">用户信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckEmailUnique(SysUserBo user)
	{
		var exist = await fsql.Select<TSysUser>()
			.Where(x => x.Email == user.Email)
			.WhereIf(user.UserId.HasValue, x => x.UserId != user.UserId)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验用户是否允许操作（对应 Java 的 <c>checkUserAllowed</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	public void CheckUserAllowed(long? userId)
	{
		if (userId.HasValue && userId.Value == SystemConstants.SUPER_ADMIN_USER_ID)
		{
			throw new ServiceException("不允许操作超级管理员用户");
		}
	}

	/// <summary>
	/// 校验用户是否有数据权限（对应 Java 的 <c>checkUserDataScope</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	public async Task CheckUserDataScope(long? userId)
	{
		if (!userId.HasValue)
		{
			return;
		}

		var loginUser = await loginService.GetLoginUser();
		if (loginUser != null && loginUser.IsSuperAdmin())
		{
			return;
		}

		// Java 通过 DataPermissionHelper 在 countUserById 上套用户数据权限；C# 端暂无该设施，
		// 退化为判断用户是否存在（无数据权限约束时等价于可见全部用户）
		if (!await fsql.Select<TSysUser>().Where(x => x.UserId == userId.Value).AnyAsync())
		{
			throw new ServiceException("没有权限访问用户数据！");
		}
	}

	/// <summary>
	/// 新增用户（对应 Java 的 <c>insertUser</c>）
	/// </summary>
	/// <param name="user">用户信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> InsertUser(SysUserBo user)
	{
		var sysUser = user.MapTo<TSysUser>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		sysUser.CreateBy ??= await loginService.GetLoginuid();

		var rows = await fsql.Insert(sysUser).ExecuteAffrowsAsync();
		// 主键由 SnowflakeAop 生成并回写到实体，这里直接取用
		await InsertUserPost(user.PostIds, sysUser.UserId);
		await InsertUserRole(user.RoleIds, sysUser.UserId);
		return rows;
	}

	/// <summary>
	/// 修改用户（对应 Java 的 <c>updateUser</c>）
	/// </summary>
	/// <param name="user">用户信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateUser(SysUserBo user)
	{
		var userId = user.UserId ?? 0L;

		// 删除用户与角色的关联后重建
		await fsql.Delete<TSysUserRole>().Where(x => x.UserId == userId).ExecuteAffrowsAsync();
		await InsertUserRole(user.RoleIds, userId);

		// 删除用户与岗位的关联后重建
		await fsql.Delete<TSysUserPost>().Where(x => x.UserId == userId).ExecuteAffrowsAsync();
		await InsertUserPost(user.PostIds, userId);

		var sysUser = user.MapTo<TSysUser>(mapper);
		sysUser.UpdateBy ??= await loginService.GetLoginuid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义，
		// 未传的字段（创建人/创建时间/密码等）为 null 时不会被覆盖
		return await fsql.Update<TSysUser>()
			.SetSourceIgnore(sysUser)
			.Where(a => a.UserId == userId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 修改用户状态（对应 Java 的 <c>updateUserStatus</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="status">账号状态</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateUserStatus(long userId, string status)
	{
		return await fsql.Update<TSysUser>()
			.Set(x => x.Status, status)
			.Set(x => x.UpdateTime, DateTime.Now)
			.Set(x => x.UpdateBy, await loginService.GetLoginuid())
			.Where(x => x.UserId == userId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 重置用户密码（对应 Java 的 <c>resetUserPwd</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="password">加密后的密码</param>
	/// <returns>影响行数</returns>
	public async Task<int> ResetUserPwd(long userId, string password)
	{
		return await fsql.Update<TSysUser>()
			.Set(x => x.Password, password)
			.Set(x => x.UpdateTime, DateTime.Now)
			.Set(x => x.UpdateBy, await loginService.GetLoginuid())
			.Where(x => x.UserId == userId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 批量删除用户（对应 Java 的 <c>deleteUserByIds</c>）
	/// </summary>
	/// <param name="userIds">用户ID数组</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteUserByIds(long[] userIds)
	{
		foreach (var userId in userIds)
		{
			CheckUserAllowed(userId);
		}

		// 删除用户与角色 / 岗位的关联
		await fsql.Delete<TSysUserRole>().Where(x => userIds.Contains(x.UserId)).ExecuteAffrowsAsync();
		await fsql.Delete<TSysUserPost>().Where(x => userIds.Contains(x.UserId)).ExecuteAffrowsAsync();

		// 对应 Java 的 @TableLogic 逻辑删除：deleteBatchIds 实际是 update del_flag = '1'
		return await fsql.Update<TSysUser>()
			.Set(x => x.DelFlag, DEL_FLAG_DELETED)
			.Where(x => userIds.Contains(x.UserId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 用户授权角色（对应 Java 的 <c>insertUserAuth</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="roleIds">角色ID数组</param>
	public async Task InsertUserAuth(long userId, long[] roleIds)
	{
		await fsql.Delete<TSysUserRole>().Where(x => x.UserId == userId).ExecuteAffrowsAsync();
		await InsertUserRole(roleIds, userId);
	}

	/// <summary>
	/// 用户导入（对应 Java 的 <c>SysUserImportListener</c>）
	/// </summary>
	/// <param name="userList">Excel 解析出的用户列表</param>
	/// <param name="isUpdateSupport">是否更新已存在的用户</param>
	/// <param name="operUserId">操作人ID</param>
	/// <returns>导入结果说明；存在失败数据时抛出 <see cref="ServiceException"/></returns>
	public async Task<string> ImportUser(List<SysUserImportVo> userList, bool isUpdateSupport, long? operUserId)
	{
		var initPassword = await configService.SelectConfigByKey("sys.user.initPassword");
		var password = BC.HashPassword(initPassword ?? string.Empty);
		var successNum = 0;
		var failureNum = 0;
		var successMsg = new StringBuilder();
		var failureMsg = new StringBuilder();

		// 部门名称 → 部门ID（对应 Java 的 DeptExcelConverter 读方向转换）
		var deptPathNames = await deptService.SelectDeptPathNames();
		var deptNameToId = deptPathNames.GroupBy(x => x.Value)
			.ToDictionary(x => x.Key, x => x.First().Key);

		foreach (var userVo in userList)
		{
			var username = userVo.UserName;
			try
			{
				// Excel 中的字典标签 / 部门名称 → 字段值
				await dictFormatConverter.ToFieldValue(userVo);
				if (userVo.DeptName.IsNotNullOrWhiteSpace())
				{
					if (!deptNameToId.TryGetValue(userVo.DeptName, out var deptId))
					{
						throw new ServiceException($"部门不存在：{userVo.DeptName}");
					}

					userVo.DeptId = deptId;
				}

				var sysUser = await SelectUserByUserName(username);
				if (sysUser == null)
				{
					// 新增用户
					var user = userVo.MapTo<SysUserBo>(mapper);
					user.Password = password;
					user.CreateBy = operUserId;
					await InsertUser(user);
					successNum++;
					successMsg.Append("<br/>").Append(successNum).Append("、账号 ").Append(username).Append(" 导入成功");
				}
				else if (isUpdateSupport)
				{
					// 更新用户
					var user = userVo.MapTo<SysUserBo>(mapper);
					user.UserId = sysUser.UserId;
					user.UpdateBy = operUserId;
					await UpdateUser(user);
					successNum++;
					successMsg.Append("<br/>").Append(successNum).Append("、账号 ").Append(username).Append(" 更新成功");
				}
				else
				{
					failureNum++;
					failureMsg.Append("<br/>").Append(failureNum).Append("、账号 ").Append(username).Append(" 已存在");
				}
			}
			catch (Exception e)
			{
				failureNum++;
				failureMsg.Append("<br/>").Append(failureNum).Append("、账号 ").Append(username).Append(" 导入失败：")
					.Append(e.Message);
			}
		}

		// 对应 Java ExcelResult.getAnalysis()
		if (failureNum > 0)
		{
			var errorMsg = $"很抱歉，导入失败！共 {failureNum} 条数据格式不正确，错误如下：";
			throw new ServiceException(errorMsg + failureMsg);
		}

		successMsg.Insert(0, $"恭喜您，数据已全部导入成功！共 {successNum} 条，数据如下：");
		return successMsg.ToString();
	}

	/// <summary>
	/// 新增用户岗位关联（对应 Java 的 <c>insertUserPost</c>）
	/// </summary>
	/// <param name="postIds">岗位ID数组</param>
	/// <param name="userId">用户ID</param>
	private async Task InsertUserPost(long[] postIds, long userId)
	{
		if (postIds == null || postIds.Length == 0)
		{
			return;
		}

		var list = postIds.Select(postId => new TSysUserPost { UserId = userId, PostId = postId }).ToList();
		await fsql.Insert(list).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 新增用户角色关联（对应 Java 的 <c>insertUserRole</c>）
	/// </summary>
	/// <param name="roleIds">角色ID数组</param>
	/// <param name="userId">用户ID</param>
	private async Task InsertUserRole(long[] roleIds, long userId)
	{
		if (roleIds == null || roleIds.Length == 0)
		{
			return;
		}

		// Java 在这里会调用 roleService.checkRoleDataScope 校验角色数据权限，C# 端无数据权限设施，未实现
		var list = roleIds.Select(roleId => new TSysUserRole { UserId = userId, RoleId = roleId }).ToList();
		await fsql.Insert(list).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 构造用户列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="user">用户筛选条件</param>
	/// <returns>用户列表查询对象</returns>
	private ISelect<TSysUser> BuildUserQuery(SysUserBo user)
	{
		var q = fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereNotNullEq(user.UserId, x => x.UserId)
			.WhereNotNullEq(user.DeptId, x => x.DeptId)
			.WhereLike(user.UserName, x => x.UserName)
			.WhereLike(user.NickName, x => x.NickName)
			.WhereHasTextEq(user.UserType, x => x.UserType)
			.WhereLike(user.Email, x => x.Email)
			.WhereLike(user.PhoneNumber, x => x.PhoneNumber)
			.WhereHasTextEq(user.Status, x => x.Status)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(user.Params, x => x.CreateTime);

		// 对应 Java 的 in(userIds) / notIn(excludeUserIds)
		var userIds = ParseLongList(user.UserIds);
		if (userIds.Count > 0)
		{
			q = q.Where(x => userIds.Contains(x.UserId));
		}

		var excludeUserIds = ParseLongList(user.ExcludeUserIds);
		if (excludeUserIds.Count > 0)
		{
			q = q.Where(x => !excludeUserIds.Contains(x.UserId));
		}

		// 对应 Java 的 inSql("select user_id from sys_user_role where role_id = #{roleId}")，
		// 这里先取出用户ID再按 in 过滤，语义一致
		if (user.RoleId.HasValue)
		{
			var roleUserIds = fsql.Select<TSysUserRole>()
				.Where(r => r.RoleId == user.RoleId.Value)
				.ToList(r => r.UserId);
			q = roleUserIds.Count == 0 ? q.Where(x => false) : q.Where(x => roleUserIds.Contains(x.UserId));
		}

		return q.OrderBy(x => x.UserId);
	}

	/// <summary>
	/// 构造「用户 + 角色」关联查询条件（对应 Java 的 <c>buildUserRoleJoinWrapper</c>）
	/// </summary>
	/// <param name="user">用户筛选条件</param>
	/// <returns>用户列表查询对象</returns>
	/// <remarks>
	/// Java 侧通过 left join sys_dept / sys_user_role / sys_role 并 distinct 实现；
	/// C# 端按用户维度查询，角色关联由调用方以 in / notIn 过滤，语义等价且避免 join 去重。
	/// </remarks>
	private ISelect<TSysUser> BuildUserRoleJoinQuery(SysUserBo user)
	{
		return fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereLike(user.UserName, x => x.UserName)
			.WhereHasTextEq(user.Status, x => x.Status)
			.WhereLike(user.PhoneNumber, x => x.PhoneNumber)
			.OrderBy(x => x.UserId);
	}

	/// <summary>
	/// 回填用户列表的部门名称（对应 Java 的 left join sys_dept）
	/// </summary>
	/// <param name="rows">用户列表</param>
	private async Task FillDeptName(List<SysUserVo> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		var deptIds = rows.Where(x => x.DeptId.HasValue).Select(x => x.DeptId.Value).Distinct().ToList();
		if (deptIds.Count == 0)
		{
			return;
		}

		var depts = await fsql.Select<TSysDept>()
			.Where(x => deptIds.Contains(x.DeptId))
			.ToListAsync(x => new { x.DeptId, x.DeptName });
		var deptNames = depts.ToDictionary(x => x.DeptId, x => x.DeptName);

		foreach (var row in rows)
		{
			if (row.DeptId.HasValue && deptNames.TryGetValue(row.DeptId.Value, out var deptName))
			{
				row.DeptName = deptName;
			}
		}
	}

	/// <summary>
	/// 回填导出数据的部门负责人账号（对应 Java 的 left join sys_user u1 on u1.user_id = d.leader）
	/// </summary>
	/// <param name="users">用户实体列表</param>
	/// <param name="rows">导出数据行</param>
	private async Task FillLeaderName(List<TSysUser> users, List<SysUserExportVo> rows)
	{
		var deptIds = users.Where(x => x.DeptId.HasValue).Select(x => x.DeptId.Value).Distinct().ToList();
		if (deptIds.Count == 0)
		{
			return;
		}

		var depts = await fsql.Select<TSysDept>()
			.Where(x => deptIds.Contains(x.DeptId))
			.ToListAsync(x => new { x.DeptId, x.Leader });
		var deptLeaders = depts.Where(x => x.Leader.HasValue)
			.ToDictionary(x => x.DeptId, x => x.Leader.Value);

		var leaderIds = deptLeaders.Values.Distinct().ToList();
		if (leaderIds.Count == 0)
		{
			return;
		}

		var leaders = await fsql.Select<TSysUser>()
			.Where(x => leaderIds.Contains(x.UserId))
			.ToListAsync(x => new { x.UserId, x.UserName });
		var leaderNames = leaders.ToDictionary(x => x.UserId, x => x.UserName);

		for (var i = 0; i < users.Count; i++)
		{
			if (users[i].DeptId.HasValue
			    && deptLeaders.TryGetValue(users[i].DeptId.Value, out var leaderId)
			    && leaderNames.TryGetValue(leaderId, out var leaderName))
			{
				rows[i].LeaderName = leaderName;
			}
		}
	}

	/// <summary>
	/// 解析逗号分隔的ID串（对应 Java 的 <c>StringUtils.splitTo(value, Convert::toLong)</c>）
	/// </summary>
	/// <param name="value">逗号分隔的ID串</param>
	/// <returns>ID列表</returns>
	private static List<long> ParseLongList(string value)
	{
		if (value.IsNullOrWhiteSpace())
		{
			return [];
		}

		return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(item => long.TryParse(item, out var id) ? id : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id.Value)
			.ToList();
	}
}
