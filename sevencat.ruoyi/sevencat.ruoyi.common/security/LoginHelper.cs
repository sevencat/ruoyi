using Autofac.Util;

namespace sevencat.ruoyi.common.security;

public class LoginHelper
{
	public static readonly Lazy<ILoginService> loginService = IocFactory.CreateLazy<ILoginService>();

	public static async Task<long?> GetLoginUid()
	{
		return await loginService.Value.GetLoginuid();
	}
}