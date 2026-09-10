using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.captcha.config;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.vo;
using SixLabors.ImageSharp;
using SixLaborsCaptcha.Core;
using ZiggyCreatures.Caching.Fusion;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/auth")]
public class AuthController(
	CaptchaProperties captchaProperties,
	SixLaborsCaptchaModule slc,
	IFusionCache cache)
{
	[HttpGet("code")]
	public async Task<CommonResult<CaptchaVo>> Code()
	{
		if (!captchaProperties.Enabled)
		{
			return CommonResult.Success(new CaptchaVo(false, null, null));
		}

		var uuid = Guid.NewGuid().ToString("N");
		var verifyKey = GlobalConstants.CAPTCHA_CODE_KEY + uuid;
		var code = Extensions.GetUniqueKey(6);
		await cache.SetAsync(verifyKey, code,
			options => options.SetDuration(TimeSpan.FromMinutes(2)));
		var result = slc.Generate(code);
		//这里要加进cache
		return CommonResult.Success(new CaptchaVo(true, uuid, Convert.ToBase64String(result)));
	}

	public record CaptchaVo(bool captchaEnabled, string uuid, string img);

	[HttpPost("login")]
	public CommonResult<LoginVo> Login([FromBody] LoginBody req)
	{
		return CommonResult.Fail("暂未实现");
	}
}