using Minio;
using Minio.DataModel.Args;
using sevencat.common;
using sevencat.ruoyi.common.oss;

namespace sevencat.ruoyi.config.impl;

/// <summary>
/// 基于 MinIO SDK 的对象存储实现（<see cref="IOssClient"/> 的默认实现）
/// </summary>
/// <remarks>
/// 把 <see cref="IOssClient"/> 的三个方法直接映射到 <see cref="IMinioClient"/> 的
/// Put/Get/RemoveObject，MinIO 的 Args 构造器只在本类内部出现，不会泄漏到业务层。
/// </remarks>
public class MinioOssClient(IMinioClient minioClient) : IOssClient
{
	/// <inheritdoc />
	public Task PutObjectAsync(string bucket, string objectName, Stream data, long size, string contentType,
		CancellationToken cancellationToken = default)
	{
		var args = new PutObjectArgs()
			.WithBucket(bucket)
			.WithObject(objectName)
			.WithStreamData(data)
			// size 为 -1 时表示长度未知，SDK 会自动走分片上传
			.WithObjectSize(size);
		if (contentType.IsNotNullOrWhiteSpace())
		{
			args = args.WithContentType(contentType);
		}

		return minioClient.PutObjectAsync(args, cancellationToken);
	}

	/// <inheritdoc />
	public Task GetObjectAsync(string bucket, string objectName, Action<Stream> callback,
		CancellationToken cancellationToken = default)
	{
		var args = new GetObjectArgs()
			.WithBucket(bucket)
			.WithObject(objectName)
			.WithCallbackStream(callback);
		return minioClient.GetObjectAsync(args, cancellationToken);
	}

	/// <inheritdoc />
	public Task RemoveObjectAsync(string bucket, string objectName, CancellationToken cancellationToken = default)
	{
		var args = new RemoveObjectArgs()
			.WithBucket(bucket)
			.WithObject(objectName);
		return minioClient.RemoveObjectAsync(args, cancellationToken);
	}
}
