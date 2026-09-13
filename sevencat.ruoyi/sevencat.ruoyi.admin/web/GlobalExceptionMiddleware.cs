using sevencat.common;
using sevencat.common.entity;
using sevencat.ruoyi.common.exception;

namespace sevencat.ruoyi.web;

public class GlobalExceptionMiddleware
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
	private readonly RequestDelegate next;

	public GlobalExceptionMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (BaseException bex)
		{
			var msg = bex.BuildMessage();
			Log.Error(bex, "业务异常:{0}", msg);
			await WriteJsonAsync(context, CommonResult.Fail(ResultCode.GLOBAL_ERROR, "异常:" + msg));
		}
		catch (CommonException ce)
		{
			Log.Error(ce, "业务异常:{0}", ce.Message);
			await WriteJsonAsync(context, ce.Reason);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "未处理异常:{0}", ex.Message);
			await WriteJsonAsync(context, CommonResult.Fail(ResultCode.GLOBAL_ERROR, "异常:" + ex.Message));
		}
	}

	/// <summary>
	/// 输出统一的 JSON 错误响应
	/// </summary>
	/// <param name="context">当前请求上下文</param>
	/// <param name="body">响应体</param>
	/// <remarks>
	/// 1. HTTP 状态码统一保持 200，业务错误由响应体中的 code 表达（沿用 Java RuoYi 的前端约定）；
	/// 2. 响应已开始写出时（例如导出流写到一半才报错）无法再改状态码和响应体，
	///    此时只记录日志并放弃写入，避免二次写响应抛 InvalidOperationException。
	/// </remarks>
	private static async Task WriteJsonAsync(HttpContext context, object body)
	{
		if (context.Response.HasStarted)
		{
			Log.Error("响应已开始写出，无法返回统一错误体，path={0}", context.Request.Path);
			return;
		}

		context.Response.ContentType = "text/json;charset=utf-8";
		await context.Response.WriteAsync(body.ToJson(), System.Text.Encoding.UTF8);
	}
}