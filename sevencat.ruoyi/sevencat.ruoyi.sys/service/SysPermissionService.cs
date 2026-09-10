using Autofac.Annotation;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysPermissionService(IFreeSql fsql)
{
	public async Task<HashSet<string>> GetRolePermission(long userId)
	{
		var roles = new HashSet<string>();
		// 管理员拥有所有权限
		if (LoginUser.IsSuperAdmin(userId))
		{
			roles.Add(SystemConstants.SUPER_ADMIN_ROLE_KEY);
		}
		else
		{
			var rolesbyuid = await SelectRolePermissionByUserId(userId);
			foreach (var r in rolesbyuid)
				roles.Add(r);
		}

		return roles;
	}

	public async Task<List<string>> SelectRolePermissionByUserId(long userid)
	{
		var rolekeys = await fsql.Select<TSysUserRole, TSysRole>()
			.LeftJoin(x => x.t1.RoleId == x.t2.RoleId)
			.Where(x => x.t1.UserId == userid)
			.ToListAsync(x => x.t2.RoleKey);
		var permsSet = new HashSet<string>();
		foreach (var rk in rolekeys)
		{
			var subrks = rk.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			foreach (var subrk in subrks)
				permsSet.Add(subrk);
		}

		return permsSet.ToList();
	}
}