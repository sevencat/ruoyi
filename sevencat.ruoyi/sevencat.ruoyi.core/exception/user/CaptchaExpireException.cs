namespace sevencat.ruoyi.core.exception.user;

public class CaptchaExpireException : UserException
{
	public CaptchaExpireException() : base("user.jcaptcha.expire")
	{
	}
}