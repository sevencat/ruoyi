using Autofac.Annotation;
using Microsoft.Extensions.Configuration;
using sevencat.common;
using sevencat.ruoyi.common.oss;

namespace sevencat.ruoyi.config.impl;

/// <summary>
/// 本地文件系统对象存储实现（无 MinIO 服务时使用）
/// </summary>
/// <remarks>
/// 目录约定与 <c>MinioMockController</c> 保持一致：桶 = 存储根目录下的一级子目录，对象 = 文件，
/// 区别是本实现直接读写文件系统（不走 HTTP / S3 协议），适合单机部署或本地联调。
/// 存储根目录取配置 <c>minio:root</c>，未配置时为 <c>{程序目录}/minio-data</c>。
/// </remarks>
[Component(typeof(LocalOssClient))]
public class LocalOssClient : IOssClient
{
	[Value("miniodrootdir")]
	private string _root;


	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="config">应用配置，读取 minio:root</param>
	public LocalOssClient()
	{
	}

	/// <inheritdoc />
	public async Task PutObjectAsync(string bucket, string objectName, Stream data, long size, string contentType,
		CancellationToken cancellationToken = default)
	{
		var filePath = ResolveObjectPath(bucket, objectName);
		// 桶目录（以及对象键里的伪目录）不存在时自动创建
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));

		await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None,
			81920, useAsync: true);
		await data.CopyToAsync(fileStream, cancellationToken);
	}

	/// <inheritdoc />
	public Task GetObjectAsync(string bucket, string objectName, Action<Stream> callback,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var filePath = ResolveObjectPath(bucket, objectName);
		if (!File.Exists(filePath))
		{
			return Task.FromException(new FileNotFoundException($"对象不存在：{bucket}/{objectName}", filePath));
		}

		// 与 MinIO 的回调语义一致：回调执行期间流可读，方法返回后流即关闭
		using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
			FileOptions.SequentialScan);
		callback(fileStream);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task RemoveObjectAsync(string bucket, string objectName, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var filePath = ResolveObjectPath(bucket, objectName);
		// 与 S3 语义一致：对象不存在也视为删除成功
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// 校验桶名/对象键并解析为物理文件绝对路径
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="objectName">对象键，可含 / 表示多级目录</param>
	/// <returns>对象文件绝对路径</returns>
	/// <exception cref="ArgumentException">桶名或对象键不合法（含越权路径）时抛出</exception>
	private string ResolveObjectPath(string bucket, string objectName)
	{
		if (bucket.IsNullOrWhiteSpace() || bucket.Contains("..")
		                                || bucket.Contains('/') || bucket.Contains('\\'))
		{
			throw new ArgumentException($"桶名不合法：{bucket}", nameof(bucket));
		}

		if (objectName.IsNullOrWhiteSpace())
		{
			throw new ArgumentException("对象键不能为空", nameof(objectName));
		}

		var bucketPath = Path.GetFullPath(Path.Combine(_root, bucket));
		string fullPath;
		try
		{
			fullPath = Path.GetFullPath(Path.Combine(bucketPath, objectName));
		}
		catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
		{
			// 对象键中出现 Windows 文件名非法字符（如 : * ?）等场景
			throw new ArgumentException($"对象键不合法：{objectName}", nameof(objectName), ex);
		}

		// 归一化后仍须位于桶目录内，避免对象键中的 ../ 越权访问其他目录
		if (!fullPath.StartsWith(bucketPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
		{
			throw new ArgumentException($"对象键越界：{objectName}", nameof(objectName));
		}

		return fullPath;
	}
}