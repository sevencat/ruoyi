using System.Net;
using Autofac.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.common.entity;

namespace sevencat.ruoyi.common.security.attr;

[AttributeUsage(AttributeTargets.Method)]
public class SaCheckPermissionAttribute(string perm) : ActionFilterAttribute
{
	private static readonly CommonResult UnAuthRsp = CommonResult.Fail(ResultCode.DENY, "没有足够权限");

	public override async Task OnActionExecutionAsync(
		ActionExecutingContext context,
		ActionExecutionDelegate next)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(next);
		var func = filterFunc.Value;
		if (func == null)
			return;
		var checkret = await func.CheckPermissions(perm);
		if (!checkret)
		{
			var result = new JsonResult(UnAuthRsp)
			{
				StatusCode = (int)HttpStatusCode.Unauthorized
			};
			context.Result = result;
			return;
		}

		await next();
	}

	private static readonly Lazy<ILoginService> filterFunc =
		IocFactory.CreateLazy<ILoginService>();
}