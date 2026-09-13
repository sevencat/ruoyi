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
/// 用户信息业务层 —— 查询条件构造、关联写入、名称回填等私有辅助实现
/// </summary>
/// <remarks>
/// 对外接口与主流程见 <c>SysUserService.cs</c>，本文件只承载不对外暴露的辅助逻辑。
/// </remarks>
public partial class SysUserService
{
	/// <summary>
	/// 新增用户岗位关联（对应 Java 的 <c>insertUserPost</c>）
	/// </summary>
	/// <param name="postIds">岗位ID数组</param>
	/// <param name="userId">用户ID</param>
	private void InsertUserPost(long[] postIds, long userId)
	{
		if (postIds == null || postIds.Length == 0)
		{
			return;
		}

		var list = postIds.Select(postId => new TSysUserPost { UserId = userId, PostId = postId }).ToList();
		fsql.Insert(list).ExecuteAffrows();
	}

	/// <summary>
	/// 新增用户角色关联（对应 Java 的 <c>insertUserRole</c>）
	/// </summary>
	/// <param name="roleIds">角色ID数组</param>
	/// <param name="userId">用户ID</param>
	private void InsertUserRole(long[] roleIds, long userId)
	{
		if (roleIds == null || roleIds.Length == 0)
		{
			return;
		}

		// Java 在这里会调用 roleService.checkRoleDataScope 校验角色数据权限，C# 端无数据权限设施，未实现
		var list = roleIds.Select(roleId => new TSysUserRole { UserId = userId, RoleId = roleId }).ToList();
		fsql.Insert(list).ExecuteAffrows();
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
