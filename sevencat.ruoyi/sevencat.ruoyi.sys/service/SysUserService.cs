using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 用户信息业务层
/// </summary>
[Component]
public class SysUserService(IFreeSql fsql, IMapper mapper)
{
	/**
     * 分页查询用户列表
     *
     * @param user      用户查询条件
     * @param pageQuery 分页参数
     * @return 用户分页列表
     */
	public async Task<PageResult<SysUserVo>> SelectPageUserList(SysUserBo user, PageQuery2 pageQuery)
	{
		var q = await BuildUserQuery(user);
		var itemlst = await q.ToPage(pageQuery);
		var result = itemlst.MapTo<SysUserVo>(mapper);

		// Java 的 selectPageUserList 未查询 password 列，这里同步置空，避免密码散列外泄
		foreach (var vo in result.Rows)
		{
			vo.Password = null;
		}

		// Java 通过 @Translation(DEPT_ID_TO_NAME) 在序列化时回填部门名称，C# 端无该机制，改为查询后回填
		await FillDeptName(result.Rows);

		return result;
	}

	/// <summary>
	/// 构造用户列表查询条件
	/// </summary>
	/// <param name="user">用户筛选条件</param>
	/// <returns>用户列表查询对象</returns>
	private async Task<ISelect<TSysUser>> BuildUserQuery(SysUserBo user)
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

		// 按角色过滤（对应 Java 的 inSql: select user_id from sys_user_role where role_id = #{roleId}）
		if (user.RoleId.HasValue)
		{
			var userIds = await fsql.Select<TSysUserRole>()
				.Where(x => x.RoleId == user.RoleId.Value)
				.ToListAsync(x => x.UserId);

			if (userIds.Count == 0)
			{
				return q.Where(x => false);
			}

			q = q.Where(x => userIds.Contains(x.UserId));
		}

		return q.OrderBy(x => x.UserId);
	}

	/// <summary>
	/// 按部门ID回填部门名称
	/// </summary>
	/// <param name="rows">用户列表</param>
	private async Task FillDeptName(List<SysUserVo> rows)
	{
		var deptIds = rows
			.Where(x => x.DeptId.HasValue)
			.Select(x => x.DeptId.Value)
			.Distinct()
			.ToList();
		if (deptIds.Count == 0)
		{
			return;
		}

		var deptNames = await fsql.Select<TSysDept>()
			.Where(x => deptIds.Contains(x.DeptId))
			.ToDictionaryAsync(x => x.DeptId, x => x.DeptName);

		foreach (var row in rows)
		{
			if (row.DeptId.HasValue && deptNames.TryGetValue(row.DeptId.Value, out var deptName))
			{
				row.DeptName = deptName;
			}
		}
	}
}
