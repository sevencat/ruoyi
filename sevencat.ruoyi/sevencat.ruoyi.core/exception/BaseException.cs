namespace sevencat.ruoyi.core.exception;

public class BaseException : Exception
{
	private string _module;
	private string _code;
	private object[] _args;

	public BaseException(string module, string code, params object[] args)
	{
		_module = module;
		_code = code;
		_args = args;
	}
}