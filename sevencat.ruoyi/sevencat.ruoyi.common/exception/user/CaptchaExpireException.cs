namespace sevencat.ruoyi.common.exception.user;

public class CaptchaExpireException : UserException
{
	public CaptchaExpireException() : base("user.jcaptcha.expire")
	{
	}
}