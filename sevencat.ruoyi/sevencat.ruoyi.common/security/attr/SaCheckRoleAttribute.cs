using System.Net;
using Autofac.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sevencat.common.entity;

namespace sevencat.ruoyi.common.security.attr;

/// <summary>
/// 角色校验特性（对应 Java 的 <c>cn.dev33.satoken.annotation.SaCheckRole</c>）
/// </summary>
/// <remarks>
/// Java 端由 Sa-Token 拦截器读取该注解并校验当前登录用户角色；
/// C# 端由本特性读取并校验 <c>LoginUser.RolePermission</c>，多个角色时满足其一即可。
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class SaCheckRoleAttribute : ActionFilterAttribute
{
	private readonly string[] _roles;

	/// <summary>
	/// 构造角色校验特性。
	/// </summary>
	/// <param name="roles">允许访问的角色标识</param>
	public SaCheckRoleAttribute(params string[] roles)
	{
		_roles = roles;
	}

	public override async Task OnActionExecutionAsync(
		ActionExecutingContext context,
		ActionExecutionDelegate next)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(next);
		var func = filterFunc.Value;
		if (func == null)
			return;
		var checkret = await func.CheckRoles(_roles);
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

	private static readonly CommonResult UnAuthRsp = CommonResult.Fail(ResultCode.DENY, "没有足够权限");

	private static readonly Lazy<ILoginService> filterFunc =
		IocFactory.CreateLazy<ILoginService>();
}
