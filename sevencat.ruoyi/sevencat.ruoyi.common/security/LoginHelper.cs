using Autofac.Util;
using sevencat.ruoyi.common.entity;

namespace sevencat.ruoyi.common.security;

public class LoginHelper
{
	public static readonly Lazy<ILoginService> loginService = IocFactory.CreateLazy<ILoginService>();

	public static async Task<long?> GetLoginUid()
	{
		return await loginService.Value.GetLoginuid();
	}

	public static LoginUser FastGetLoginUser()
	{
		return loginService.Value.FastgetLoginUser();
	}
}