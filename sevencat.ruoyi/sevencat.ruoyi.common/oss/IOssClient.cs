namespace sevencat.ruoyi.common.oss;

/// <summary>
/// 对象存储客户端抽象
/// </summary>
/// <remarks>
/// 把 <c>SysOssService</c> 中实际用到的对象存储能力收敛到本接口，用于屏蔽具体实现：
/// MinIO SDK、本地文件实现（联调用的 mock）、其他 S3 兼容网关等。
/// 桶名、服务商标识、对外访问地址前缀由实现自身提供
/// （<see cref="BucketName"/>、<see cref="Service"/>、<see cref="Url"/>），
/// 因此业务层只面向对象键编程；一个实现实例对应一个桶（取配置 <c>minio:bucket</c>），
/// 需要多桶时按桶注册不同的实现实例。
/// 对象键（<c>objectName</c>）语义与 S3 一致，可含 <c>/</c> 伪目录。
/// </remarks>
public interface IOssClient
{
	/// <summary>
	/// 桶名（S3 的 bucket），实现从配置读取，如 appsettings.json 的 <c>minio:bucket</c>
	/// </summary>
	string BucketName { get; }

	/// <summary>
	/// 服务商标识，写入 <c>sys_oss.service</c>，如 <c>minio</c>、<c>local</c>
	/// </summary>
	string Service { get; }

	/// <summary>
	/// 对象的对外访问地址前缀（如 <c>http://127.0.0.1:7050/api/minio</c>），
	/// 用于拼出 <c>{前缀}/{桶名}/{对象键}</c>
	/// </summary>
	string Url { get; }

	/// <summary>
	/// 上传对象
	/// </summary>
	/// <param name="objectName">对象键</param>
	/// <param name="data">对象内容流，读到流末尾即对象结束</param>
	/// <param name="size">对象字节数，长度未知时传 -1</param>
	/// <param name="contentType">MIME 类型，可为空</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task PutObjectAsync(string objectName, Stream data, long size, string contentType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 下载对象
	/// </summary>
	/// <param name="objectName">对象键</param>
	/// <param name="callback">回调，形参为对象内容流，仅在回调执行期间可读</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task GetObjectAsync(string objectName, Action<Stream> callback,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 删除对象
	/// </summary>
	/// <param name="objectName">对象键</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task RemoveObjectAsync(string objectName, CancellationToken cancellationToken = default);
}
