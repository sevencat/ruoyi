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
	SysPermissionService sysPermissionService) : ILoginService
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	public async Task<LoginVo> login(LoginBody loginBody)
	{
		var username = loginBody.username;
		var pwd = loginBody.password;
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
		var cachekey = GlobalConstants.USER_TOKEN_KEY + token;
		await cache.SetAsync(cachekey, loginUser, x => x.SetDuration(TimeSpan.FromDays(1)));

		var loginVo = new LoginVo();
		loginVo.AccessToken = token;
		loginVo.expireqIn = 3600 * 24;
		loginVo.ClientId = loginBody.clientId;
		return loginVo;
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