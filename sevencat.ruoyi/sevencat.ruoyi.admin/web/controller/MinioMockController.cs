using Autofac.Annotation;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;

namespace sevencat.ruoyi.web.controller;

/// <summary>
/// 本地对象存储（用本地文件系统模拟 MinIO 的 S3 接口） 如果有正式的，就把这个去掉!!!
/// </summary>
/// <remarks>
/// 桶 = 存储根目录下的一级子目录，对象 = 文件；请求体直接落盘，不做签名校验，
/// 供前端 / SDK 在无 MinIO 服务时联调使用，请勿直接暴露到公网。
/// 为什么对象相关动作同时挂两份路由（~/{bucket}/{**key} 与 /api/minio/{bucket}/{**key}）：
/// Minio 的 .NET SDK 在 WithEndpoint 时只保留 host:port（MinioClientExtensions.SetBaseURL 只取
/// Host/Port，endpoint 里的路径会被丢弃），它发出的请求固定是 /{bucket}/{object}，即**根路径**；
/// 而浏览器直连用的地址来自配置 minio:url（本例 http://127.0.0.1:7050/api/minio）。
/// 只挂 /api/minio 时，SDK 的上传/下载请求会因根路径没有路由而拿到 404（无响应体），
/// 表现为 Minio SDK 抛出 MinIO API responded with message={对象键}。
/// </remarks>
[ApiController]
[Route("/api/minio")]
public class MinioMockController : ControllerBase
{
	[Value("miniodrootdir")]
	private string LocalStorageRoot;

	/// <summary>
	/// 模拟 PutObject（上传文件 / 写入对象）
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="key">对象键，可含 / 表示多级目录</param>
	/// <returns>操作结果</returns>
	[HttpPut("{bucket}/{**key}")]
	public async Task<IActionResult> PutObject([FromRoute] string bucket, [FromRoute] string key)
	{
		if (!TryResolveObjectPath(bucket, key, out var filePath))
		{
			return BadRequest(CommonResult.Fail("桶名或对象键不合法"));
		}

		// 自动创建物理“桶”和子目录
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));

		// 直接将 ASP.NET Core 的 HTTP 请求体（二进制流）写入本地文件系统
		await using (var fileStream = System.IO.File.Create(filePath))
		{
			await Request.Body.CopyToAsync(fileStream);
		}


		/// 2. 生成符合 S3 规范的 ETag（必须带双引号）
		var mockETag = $"\"{Guid.NewGuid():N}\"";

		// 关键：必须塞进这两个 Header
		Response.Headers.Append("ETag", mockETag);
		Response.Headers.Append("Server", "MinIO");

		// 3. 终极修正：返回 NoContent() (HTTP 204) 或者返回一个没有任何 Body 的 Ok() (HTTP 200)
		// 绝大多数 S3 协议规范在 PutObject 成功后会返回 200 OK，但 Body 的 Content-Length 必须为 0
		return Ok();
	}

	/// <summary>
	/// 模拟 GetObject（下载文件 / 读取对象）
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="key">对象键</param>
	/// <returns>文件；对象不存在时返回 404 + NoSuchKey</returns>
	[HttpGet("{bucket}/{**key}")]
	public IActionResult GetObject([FromRoute] string bucket, [FromRoute] string key)
	{
		if (!TryResolveObjectPath(bucket, key, out var filePath) || !System.IO.File.Exists(filePath))
		{
			return NotFound(new { Message = "NoSuchKey", Code = "NoSuchKey" });
		}

		// 直接以物理文件流返回，完全兼容 MinIO 客户端的读取
		return PhysicalFile(filePath, "application/octet-stream");
	}

	/// <summary>
	/// 模拟 DeleteObject（删除对象）
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="key">对象键</param>
	/// <returns>操作结果（对象不存在也返回 204，与 S3 语义一致）</returns>
	[HttpDelete("{bucket}/{**key}")]
	public IActionResult DeleteObject([FromRoute] string bucket, [FromRoute] string key)
	{
		if (TryResolveObjectPath(bucket, key, out var filePath) && System.IO.File.Exists(filePath))
		{
			System.IO.File.Delete(filePath);
		}

		return NoContent();
	}

	/// <summary>
	/// 模拟 CreateBucket（创建桶，其实就是建文件夹）
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <returns>操作结果</returns>
	[HttpPut("{bucket}")]
	public IActionResult CreateBucket([FromRoute] string bucket)
	{
		if (!TryResolveBucketPath(bucket, out var bucketPath))
		{
			return BadRequest(CommonResult.Fail("桶名不合法"));
		}

		Directory.CreateDirectory(bucketPath);
		return Ok();
	}

	/// <summary>
	/// 解析本地存储根目录
	/// </summary>
	/// <param name="cfg">应用配置</param>
	/// <returns>存储根目录绝对路径</returns>
	private static string ResolveLocalStorageRoot(IConfiguration cfg)
	{
		var root = cfg.GetValue<string>("minio:root");
		if (!string.IsNullOrWhiteSpace(root))
		{
			return Path.GetFullPath(root);
		}

		// 与 AppModule 中静态文件目录一样，取程序所在目录
		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minio-data");
	}

	/// <summary>
	/// 校验并解析桶对应的物理目录
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="bucketPath">桶目录绝对路径</param>
	/// <returns>是否合法</returns>
	private bool TryResolveBucketPath(string bucket, out string bucketPath)
	{
		bucketPath = null;
		if (string.IsNullOrWhiteSpace(bucket) || bucket.Contains("..")
		                                      || bucket.Contains('/') || bucket.Contains('\\'))
		{
			return false;
		}

		bucketPath = Path.GetFullPath(Path.Combine(LocalStorageRoot, bucket));
		return true;
	}

	/// <summary>
	/// 校验并解析对象对应的物理文件路径
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="key">对象键</param>
	/// <param name="filePath">对象文件绝对路径</param>
	/// <returns>是否合法</returns>
	private bool TryResolveObjectPath(string bucket, string key, out string filePath)
	{
		filePath = null;
		if (string.IsNullOrWhiteSpace(key) || !TryResolveBucketPath(bucket, out var bucketPath))
		{
			return false;
		}

		try
		{
			var fullPath = Path.GetFullPath(Path.Combine(bucketPath, key));
			// 归一化后仍须位于桶目录内，避免 key 中的 ../ 越权访问其他目录
			if (!fullPath.StartsWith(bucketPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
			{
				return false;
			}

			filePath = fullPath;
			return true;
		}
		catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
		{
			// key 中出现 Windows 文件名非法字符（如 : * ?）等场景
			return false;
		}
	}
}