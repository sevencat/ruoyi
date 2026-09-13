using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace sevencat.ruoyi.common.util;

//平时使用的IocFactory一般用来取全局的，这个是用来取http请求过程中的
public static class HttpUtil
{
	private static readonly Lazy<IHttpContextAccessor> _lazy = IocFactory.CreateLazy<IHttpContextAccessor>();

	public static HttpContext GetCurrentHttpContext()
	{
		return _lazy.Value.HttpContext;
	}

	[return: NotNull]
	public static IServiceProvider GetScopeSp()
	{
		var ctx = _lazy.Value.HttpContext;
		if (ctx == null)
			throw new Exception("不在http中");
		return ctx.RequestServices;
	}

	[return: NotNull]
	public static ILifetimeScope GetScopeLts()
	{
		return GetScopeSp().GetService<ILifetimeScope>();
	}
}