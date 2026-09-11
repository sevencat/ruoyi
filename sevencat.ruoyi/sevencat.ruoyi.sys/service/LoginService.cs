using Autofac.Annotation;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using sevencat.common;
using sevencat.ruoyi.common.captcha.config;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception.user;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.util;
using sevencat.ruoyi.sys.vo;
using ZiggyCreatures.Caching.Fusion;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.service;

[Component]
public class LoginService(
	IMapper mapper,
	IFreeSql fsql,
	IHttpContextAccessor httpCtxAccessor,
	CaptchaProperties captchaProperties,
	IFusionCache cache,
	SysPermissionService sysPermissionService,
	SysLoginInfoService loginInfoService) : ILoginService
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 令牌有效期
	/// </summary>
	private static readonly TimeSpan TokenExpire = TimeSpan.FromDays(1);

	/// <summary>
	/// 登录成功提示消息（对应 Java 的 <c>MessageUtils.message("user.login.success")</c>）
	/// </summary>
	private const string LOGIN_SUCCESS_MESSAGE = "登录成功";

	public async Task<LoginVo> login(LoginBody loginBody)
	{
		var username = loginBody.username;
		var pwd = loginBody.password;
		try
		{
			// 校验码校验：对应 Java 的 validateCaptcha
			if (captchaProperties.Enabled)
			{
				await ValidateCaptcha(username, loginBody.code, loginBody.uuid);
			}

			var dbuser = await fsql.Select<TSysUser>()
				.Where(x => x.UserName == username)
				.FirstAsync();
			if (dbuser == null)
			{
				Log.Info("登录用户{0}不存在", username);
				throw new UserException("user.not.exists", username);
			}

			if (dbuser.Status == SystemConstants.DISABLE)
			{
				Log.Info("登录用户：{0} 已被停用.", username);
				throw new UserException("user.blocked", username);
			}

			if (!BC.Verify(pwd, dbuser.Password))
			{
				Log.Info("登录用户：{0} 密码错误.", username);
				throw new UserException("user.password.retry.limit.exceed", username);
			}

			var loginUser = await BuildLoginUser(dbuser);
			var token = Guid.NewGuid().ToString("N");
			loginUser.Token = token;
			FillLoginInfo(loginUser, loginBody.clientId);
			var cachekey = GlobalConstants.USER_TOKEN_KEY + token;
			await cache.SetAsync(cachekey, loginUser, x => x.SetDuration(TokenExpire));

			// 对应 Java 的 AsyncFactory.recordLogininfor(LOGIN_SUCCESS) 与 UserLoginSuccessListener 写回最近登录信息
			await loginInfoService.RecordLoginInfo(username, loginBody.clientId, SystemConstants.NORMAL,
				LOGIN_SUCCESS_MESSAGE, loginUser);
			await UpdateLastLoginInfo(dbuser.UserId, loginUser);

			var loginVo = new LoginVo();
			loginVo.AccessToken = token;
			loginVo.expireqIn = (long)TokenExpire.TotalSeconds;
			loginVo.ClientId = loginBody.clientId;
			return loginVo;
		}
		catch (UserException ex)
		{
			// 对应 Java 的 AsyncFactory.recordLogininfor(LOGIN_FAIL)：
			// 失败时尚未构建登录用户，终端信息由服务内部直接从请求上下文取
			await loginInfoService.RecordLoginInfo(username, loginBody.clientId, SystemConstants.DISABLE,
				ex.BuildMessage());
			throw;
		}
	}

	/// <summary>
	/// 更新用户最近登录信息（对应 Java 的 <c>UserLoginSuccessListener</c> 中写回 sys_user 的部分）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="loginUser">登录用户</param>
	private async Task UpdateLastLoginInfo(long userId, LoginUser loginUser)
	{
		await fsql.Update<TSysUser>()
			.Set(x => x.LoginIp, loginUser.Ipaddr)
			.Set(x => x.LoginDate, DateTime.Now)
			.Where(x => x.UserId == userId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 补充登录终端信息（对应 Java 的 <c>LoginHelper.fillRequestContext</c> 与 <c>UserLoginSuccessListener.handleLoginSuccess</c>）。
	/// </summary>
	/// <param name="loginUser">登录用户</param>
	/// <param name="clientId">客户端id</param>
	private void FillLoginInfo(LoginUser loginUser, string clientId)
	{
		var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		loginUser.LoginTime = now;
		loginUser.ExpireTime = now + (long)TokenExpire.TotalMilliseconds;
		// Java 端在线会话的 clientKey 取自登录参数中的客户端id
		loginUser.ClientKey = clientId;
		// TODO Java 端 deviceType 取自 sys_client.device_type，C# 端 SysClient 尚未实现，暂固定为 pc
		loginUser.DeviceType = DeviceType.PC;

		var httpctx = httpCtxAccessor.HttpContext;
		if (httpctx == null)
		{
			return;
		}

		var ip = ServletUtils.GetClientIp(httpctx);
		loginUser.Ipaddr = ip;
		if (ip.IsNotNullOrWhiteSpace())
		{
			loginUser.LoginLocation = AddressUtils.GetRealAddressByIP(ip);
		}

		var (browser, os) = UserAgentUtils.Parse(ServletUtils.GetUserAgent(httpctx));
		loginUser.Browser = browser;
		loginUser.Os = os;
	}


	private const string TokenHttpKey = "token_in_http";

	public async Task<LoginUser> GetLoginUser()
	{
		var httpctx = httpCtxAccessor.HttpContext;
		if (httpctx == null)
			return null;
		return await TryGetLoginModel(httpctx);
	}

	public async Task<LoginUser> TryGetLoginModel(HttpContext httpctx)
	{
		if (httpctx.Items.TryGetValue(TokenHttpKey, out var loginmodel))
		{
			return (LoginUser)loginmodel;
		}

		var token = TryGetToken(httpctx);
		if (string.IsNullOrWhiteSpace(token))
			return null;
		//这里有可能是Bearer
		if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		{
			token = token.AsSpan(7).ToString();
		}

		//从本地cache里拿
		var cachekey = GlobalConstants.USER_TOKEN_KEY + token;
		var getrsp = await cache.GetOrDefaultAsync<LoginUser>(cachekey);
		//3.写到http里面，供当前session使用，或者可以用autofac的这个来用。
		if (getrsp != null)
		{
			httpctx.Items.Add(TokenHttpKey, getrsp);
		}

		return getrsp;
	}

	public string TryGetToken(HttpContext httpctx)
	{
		if (httpctx.Request.Headers.TryGetValue("authorization", out var values))
		{
			if (values.Count > 0)
				return values.First();
		}

		return null;
	}

	//检查校验码
	private async Task ValidateCaptcha(string username, string code, string uuid)
	{
		var verifyKey = GlobalConstants.CAPTCHA_CODE_KEY + uuid;
		var captcha = await cache.GetOrDefaultAsync<string>(verifyKey);
		await cache.RemoveAsync(verifyKey);
		if (captcha.IsNullOrWhiteSpace())
			throw new CaptchaExpireException();
		if (captcha != code)
			throw new CaptchaException();
	}

	public async Task<LoginUser> BuildLoginUser(TSysUser dbuser)
	{
		var loginUser = new LoginUser
		{
			UserId = dbuser.UserId,
			DeptId = dbuser.DeptId,
			Username = dbuser.UserName,
			Nickname = dbuser.NickName,
			UserType = dbuser.UserType
		};
		if (loginUser.DeptId != null)
		{
			var dbdept = await fsql.Select<TSysDept>().Where(x => x.DeptId == dbuser.DeptId).FirstAsync();
			if (dbdept != null)
			{
				loginUser.DeptName = dbdept.DeptName;
				loginUser.DeptCategory = dbdept.DeptCategory;
			}
		}

		// TODO 部门信息：deptId 不为空时查询部门，填充 DeptName / DeptCategory（待 DeptService 就绪）
		//角色id
		var dbuserRoles = await fsql.Select<TSysUserRole, TSysRole>()
			.LeftJoin(x => x.t1.RoleId == x.t2.RoleId)
			.Where(x => x.t1.UserId == dbuser.UserId)
			.ToListAsync(x => x.t2);
		loginUser.Roles = dbuserRoles.Select(x => new RoleDTO()
		{
			RoleId = x.RoleId,
			RoleKey = x.RoleKey,
			RoleName = x.RoleName,
			DataScope = x.DataScope,
		}).ToList();
		if (loginUser.IsSuperAdmin())
		{
			loginUser.RolePermission = [SystemConstants.SUPER_ADMIN_ROLE_KEY];
		}
		else
		{
			var subrklist = loginUser.Roles.Select(x => x.RoleKey.Trim().Split(",",
					StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.SelectMany(x => x)
				.ToHashSet();
			loginUser.RolePermission = subrklist;
		}

		// TODO 菜单权限 MenuPermission、角色权限 RolePermission（待 PermissionService 就绪）
		if (loginUser.IsSuperAdmin())
		{
			loginUser.MenuPermission = ["*:*:*"];
		}
		else
		{
			//从roles里面拉
			var roleids = dbuserRoles.Select(x => x.RoleId).ToList();
			var menuslist = await fsql.Select<TSysRoleMenu, TSysMenu>()
				.LeftJoin(x => x.t1.MenuId == x.t2.MenuId)
				.Where(x => roleids.Contains(x.t1.RoleId))
				.ToListAsync(x => x.t2.Perms);
			loginUser.MenuPermission = menuslist.Select(x => x.Trim())
				.Where(x => x.IsNotNullOrWhiteSpace())
				.ToHashSet();
		}
		// TODO 角色列表 Roles、数据权限角色映射 DataScopeRoleMap（待 RoleService / PermissionService 就绪）

		// TODO 岗位列表 Posts（待 PostService 就绪）
		var dbposts = await fsql.Select<TSysPost, TSysUserPost>()
			.LeftJoin(x => x.t1.PostId == x.t2.PostId)
			.Where(x => x.t2.UserId == loginUser.UserId)
			.ToListAsync(x => x.t1);
		loginUser.Posts = dbposts.Select(x => x.MapTo<PostDTO>(mapper))
			.ToList();

		return loginUser;
	}

	public LoginUser FastgetLoginUser()
	{
		var httpctx = httpCtxAccessor.HttpContext;
		if (httpctx == null)
			return null;
		if (!httpctx.Items.TryGetValue(TokenHttpKey, out var loginmodel))
		{
			return null;
		}

		var lu = (LoginUser)loginmodel;
		return lu;
	}

	public async Task<long?> GetLoginuid()
	{
		var lu = await GetLoginUser();
		return lu?.UserId;
	}

	public async Task Logout()
	{
		try
		{
			var lu = await GetLoginUser();
			if (lu == null)
			{
				return;
			}

			var token = lu.Token;
			var cachekey = GlobalConstants.USER_TOKEN_KEY + token;
			await cache.RemoveAsync(cachekey);
		}
		catch (Exception ex)
		{
			// ignored
		}
	}
}