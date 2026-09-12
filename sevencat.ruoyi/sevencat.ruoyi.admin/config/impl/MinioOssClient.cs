using Autofac.Annotation;
using Minio;
using Minio.DataModel.Args;
using sevencat.common;
using sevencat.ruoyi.common.oss;
using sevencat.ruoyi.config.properties;

namespace sevencat.ruoyi.config.impl;

/// <summary>
/// 基于 MinIO SDK 的对象存储实现（<see cref="IOssClient"/> 对接真实 MinIO 时使用）
/// </summary>
/// <remarks>
/// 把 <see cref="IOssClient"/> 的三个动作直接映射到 <see cref="IMinioClient"/> 的
/// Put/Get/RemoveObject，MinIO 的 Args 构造器只在本类内部出现，不会泄漏到业务层。
/// 桶名与对外访问地址前缀取自 <see cref="MyOssConfig"/>（<c>minio:bucket</c> / <c>minio:url</c>）。
/// </remarks>
public class MinioOssClient(IMinioClient minioClient, MyOssConfig config) : IOssClient
{
	/// <inheritdoc />
	public string BucketName => config.EffectiveBucket;

	/// <inheritdoc />
	/// <remarks>与配置节名一致</remarks>
	public string Service => "minio";

	/// <inheritdoc />
	public string Url => config.Url;

	/// <inheritdoc />
	public Task PutObjectAsync(string objectName, Stream data, long size, string contentType,
		CancellationToken cancellationToken = default)
	{
		var args = new PutObjectArgs()
			.WithBucket(BucketName)
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
	public Task GetObjectAsync(string objectName, Action<Stream> callback,
		CancellationToken cancellationToken = default)
	{
		var args = new GetObjectArgs()
			.WithBucket(BucketName)
			.WithObject(objectName)
			.WithCallbackStream(callback);
		return minioClient.GetObjectAsync(args, cancellationToken);
	}

	/// <inheritdoc />
	public Task RemoveObjectAsync(string objectName, CancellationToken cancellationToken = default)
	{
		var args = new RemoveObjectArgs()
			.WithBucket(BucketName)
			.WithObject(objectName);
		return minioClient.RemoveObjectAsync(args, cancellationToken);
	}
}