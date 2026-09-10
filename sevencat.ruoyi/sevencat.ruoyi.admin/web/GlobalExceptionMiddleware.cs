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
			Log.Info("异常:{0}", msg);
			var ret = CommonResult.Fail(ResultCode.GLOBAL_ERROR, "异常:" + msg);
			context.Response.ContentType = "text/json;charset=utf-8";
			await context.Response.WriteAsync(ret.ToJson(), System.Text.Encoding.UTF8);
		}
		catch (CommonException ce)
		{
			Log.Info("异常:{0},{1}", ce.Message, ce.StackTrace);
			context.Response.ContentType = "text/json;charset=utf-8";
			await context.Response.WriteAsync(ce.Reason.ToJson(), System.Text.Encoding.UTF8);
		}
		catch (Exception ex)
		{
			Log.Info("异常:{0},{1}", ex.Message, ex.StackTrace);
			var ret = CommonResult.Fail(ResultCode.GLOBAL_ERROR, "异常:" + ex.Message);
			context.Response.ContentType = "text/json;charset=utf-8";
			await context.Response.WriteAsync(ret.ToJson(), System.Text.Encoding.UTF8);
		}
	}
}