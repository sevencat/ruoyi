namespace sevencat.ruoyi.common.exception;

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

	/// <summary>
	/// 将模块、错误码与参数拼接为一个字符串，空的部分自动跳过
	/// </summary>
	/// <remarks>
	/// 格式：<c>{module}:{code}:{arg1,arg2}</c>，例如 <c>user:user.not.exists:admin</c>
	/// </remarks>
	/// <returns>拼接后的字符串；三者都为空时返回空字符串</returns>
	public string BuildMessage()
	{
		var parts = new List<string>(3);

		if (!string.IsNullOrEmpty(_module))
		{
			parts.Add(_module);
		}

		if (!string.IsNullOrEmpty(_code))
		{
			parts.Add(_code);
		}

		if (_args is { Length: > 0 })
		{
			parts.Add(string.Join(",", _args.Select(arg => arg?.ToString() ?? "null")));
		}

		return string.Join(":", parts);
	}
}