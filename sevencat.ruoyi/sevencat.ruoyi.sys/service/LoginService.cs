using Autofac.Annotation;
using sevencat.common;
using sevencat.ruoyi.common.captcha.config;
using sevencat.ruoyi.core.constant;
using sevencat.ruoyi.core.exception.user;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;
using ZiggyCreatures.Caching.Fusion;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.service;

[Component]
public class LoginService(
	IFreeSql fsql,
	CaptchaProperties captchaProperties,
	IFusionCache cache)
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

		var loginUser = BuildLoginUser(dbuser);
		var token = Guid.NewGuid().ToString("N");
		var cachekey = GlobalConstants.USER_TOKEN_KEY + token;
		await cache.SetAsync(cachekey, loginUser, x => x.SetDuration(TimeSpan.FromDays(1)));

		var loginVo = new LoginVo();
		loginVo.AccessToken = token;
		loginVo.expireqIn = 3600 * 24;
		loginVo.ClientId = loginBody.clientId;
		return loginVo;
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

	public LoginUser BuildLoginUser(TSysUser dbuser)
	{
		var loginUser = new LoginUser
		{
			UserId = dbuser.UserId,
			DeptId = dbuser.DeptId,
			Username = dbuser.UserName,
			Nickname = dbuser.NickName,
			UserType = dbuser.UserType
		};

		// TODO 部门信息：deptId 不为空时查询部门，填充 DeptName / DeptCategory（待 DeptService 就绪）
		// TODO 菜单权限 MenuPermission、角色权限 RolePermission（待 PermissionService 就绪）
		// TODO 角色列表 Roles、数据权限角色映射 DataScopeRoleMap（待 RoleService / PermissionService 就绪）
		// TODO 岗位列表 Posts（待 PostService 就绪）

		return loginUser;
	}
}