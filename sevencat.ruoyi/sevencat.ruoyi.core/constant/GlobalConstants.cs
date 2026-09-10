namespace sevencat.ruoyi.core.constant;

public class GlobalConstants
{
	/**
 * 全局 redis key (业务无关的key)
 */
	public const string GLOBAL_REDIS_KEY = "global:";

	/**
	 * 验证码 redis key
	 */
	public const string CAPTCHA_CODE_KEY = GLOBAL_REDIS_KEY + "captcha_codes:";

	/**
	 * 防重提交 redis key
	 */
	public const string REPEAT_SUBMIT_KEY = GLOBAL_REDIS_KEY + "repeat_submit:";

	/**
	 * 限流 redis key
	 */
	public const string RATE_LIMIT_KEY = GLOBAL_REDIS_KEY + "rate_limit:";

	/**
	 * 三方认证 redis key
	 */
	public const string SOCIAL_AUTH_CODE_KEY = GLOBAL_REDIS_KEY + "social_auth_codes:";

	/**
	 * 验证码 redis key
	 */
	public const string USER_TOKEN_KEY = GLOBAL_REDIS_KEY + "user_token:";
}