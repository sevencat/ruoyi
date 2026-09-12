using Autofac.Util;
using MiniExcelLibs;
using Microsoft.AspNetCore.Http;

namespace sevencat.ruoyi.common.excel;

/// <summary>
/// 把数据以 Excel 形式写进当前 HTTP 响应流（对应 Java 的 <c>ExcelBuilder.toResponse</c>）
/// </summary>
public static class ExcelResponseWriter
{
	private static readonly Lazy<IHttpContextAccessor> httpCtxAccessor = IocFactory.CreateLazy<IHttpContextAccessor>();

	/// <summary>
	/// xlsx 内容类型
	/// </summary>
	public const string ContentType =
		"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

	/// <summary>
	/// 按给定的数据类型把数据写进响应流，下载文件名形如 <c>{sheetName}_yyyyMMddHHmmss.xlsx</c>
	/// </summary>
	/// <typeparam name="T">Excel 行类型</typeparam>
	/// <param name="response">HTTP 响应</param>
	/// <param name="rows">数据行</param>
	/// <param name="sheetName">工作表名，同时作为下载文件名前缀</param>
	public static async Task WriteAsync<T>(HttpResponse response, IEnumerable<T> rows, string sheetName)
	{
		// 先写入内存流再拷贝到响应体，避免 MiniExcel 直接写不可随机访问的响应流
		using var stream = new MemoryStream();
		await stream.SaveAsAsync(rows, printHeader: true, sheetName: sheetName,
			excelType: ExcelType.XLSX);
		stream.Position = 0;

		var fileName = $"{sheetName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
		response.ContentType = ContentType;
		response.Headers.ContentDisposition = $"attachment;filename*=utf-8''{Uri.EscapeDataString(fileName)}";
		await stream.CopyToAsync(response.Body);
	}

	public static async Task WriteExcelToHttpAsync<T>(this IEnumerable<T> rows, string sheetName)
	{
		var ctx = httpCtxAccessor.Value.HttpContext;
		if (ctx == null)
			throw new Exception("不在http请求内");
		await WriteAsync(ctx.Response, rows, sheetName);
	}
}