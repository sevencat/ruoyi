using Autofac.Util;
using Hangfire.Dashboard;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.sys.service;

namespace sevencat.ruoyi.config.impl;

public class MyDashboardFilter : IDashboardAuthorizationFilter
{
	private static readonly Lazy<LoginService> _loginService = IocFactory.CreateLazy<LoginService>();

	public bool Authorize(DashboardContext context)
	{
		var httpContext = context.GetHttpContext();
		var lu = _loginService.Value.TryGetLoginModel(httpContext).Result;
		if (lu == null)
			return false;
		return lu.RolePermission.Contains(SystemConstants.SUPER_ADMIN_ROLE_KEY);
	}
}