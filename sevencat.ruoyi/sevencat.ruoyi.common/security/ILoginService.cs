using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.security;

public interface ILoginService
{
	Task<long?> GetLoginuid();
	LoginUser FastgetLoginUser();
	Task<bool> CheckPermissions(string perm);

	/// <summary>
	/// 校验当前登录用户是否具备指定角色（对应 Java 的 Sa-Token <c>StpUtil.hasRole</c>）。
	/// 多个角色时满足其一即可。
	/// </summary>
	/// <param name="roles">角色标识列表</param>
	/// <returns>true 具备 false 不具备</returns>
	Task<bool> CheckRoles(params string[] roles);
}