using Autofac.Annotation;
using sevencat.common;
using sevencat.ruoyi.common.captcha.config;
using sevencat.ruoyi.core.exception.user;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;
using ZiggyCreatures.Caching.Fusion;
using BC = BCrypt.Net.BCrypt;

namespace sevencat.ruoyi.sys.service;

[Component]
public class PasswordAuthStrategy(
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
}