namespace sevencat.ruoyi.common.exception;

/// <summary>
/// 业务异常（对应 Java 的 <c>ServiceException</c>）
/// </summary>
/// <remarks>
/// Java 用 <c>MessageFormat.format(message, args)</c> 拼装提示语，C# 端统一交给
/// <see cref="BaseException.BuildMessage"/> 按 <c>code:args</c> 拼接，由全局异常中间件输出为失败响应。
/// </remarks>
public class ServiceException : BaseException
{
	/// <summary>
	/// 构造业务异常
	/// </summary>
	/// <param name="message">提示信息</param>
	/// <param name="args">提示信息中的占位参数</param>
	public ServiceException(string message, params object[] args) : base(null, message, args)
	{
	}
}
