using Autofac.Annotation;
using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;

namespace sevencat.ruoyi.web.controller;

/// <summary>
/// 本地对象存储（用本地文件系统模拟 MinIO 的 S3 接口）
/// </summary>
/// <remarks>
/// 桶 = 存储根目录下的一级子目录，对象 = 文件；请求体直接落盘，不做签名校验，
/// 供前端 / SDK 在无 MinIO 服务时联调使用，请勿直接暴露到公网。
/// </remarks>
[ApiController]
[Route("/api/minio")]
public class MinioController : ControllerBase
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

		// 返回标准 S3 成功响应头（ETag 是必须的，很多 SDK 会校验）
		Response.Headers.ETag = $"\"{Guid.NewGuid()}\"";
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