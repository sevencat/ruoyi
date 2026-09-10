namespace sevencat.ruoyi.core.exception.user;

public class CaptchaException : UserException
{
	public CaptchaException()
		: base("user.jcaptcha.error")
	{
	}
}