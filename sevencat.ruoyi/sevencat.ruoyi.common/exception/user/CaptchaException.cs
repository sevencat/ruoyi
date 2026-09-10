namespace sevencat.ruoyi.common.exception.user;

public class CaptchaException : UserException
{
	public CaptchaException()
		: base("user.jcaptcha.error")
	{
	}
}