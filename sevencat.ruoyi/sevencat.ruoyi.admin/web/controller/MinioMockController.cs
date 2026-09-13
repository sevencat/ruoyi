using Microsoft.AspNetCore.Mvc;
using sevencat.ruoyi.config.properties;

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
///如果使用真实的minio,这步是可以省略掉的!!!
[ApiController]
[Route("/api/minio")]
public class MinioMockController(MyOssConfig ossConfig) : ControllerBase
{
	/// <summary>
	/// 本地存储根目录绝对路径，未配置 <c>oss:root</c> 时回退为 <c>{程序目录}/minio-data</c>
	/// </summary>
	/// <remarks>与 <c>LocalOssClient</c> 共用同一条配置解析口径，避免两处行为不一致</remarks>
	private string LocalStorageRoot => ossConfig.EffectiveRoot;

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