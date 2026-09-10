namespace sevencat.ruoyi.common.exception.user;

public class UserException : BaseException
{
	public UserException(string code, params object[] args)
		: base("user", code, args)
	{
	}
}