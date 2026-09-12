namespace sevencat.ruoyi.common.oss;

/// <summary>
/// 对象存储客户端抽象
/// </summary>
/// <remarks>
/// 把 <c>SysOssService</c> 中实际用到的对象存储能力收敛到本接口，用于屏蔽具体实现：
/// MinIO SDK、本地文件实现（联调用的 mock）、其他 S3 兼容网关等。
/// <c>bucket</c> / <c>objectName</c> 的语义与 S3 一致：桶名 + 对象键（对象键可含 <c>/</c> 伪目录）。
/// </remarks>
public interface IOssClient
{
	/// <summary>
	/// 上传对象
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="objectName">对象键</param>
	/// <param name="data">对象内容流，读到流末尾即对象结束</param>
	/// <param name="size">对象字节数，长度未知时传 -1</param>
	/// <param name="contentType">MIME 类型，可为空</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task PutObjectAsync(string bucket, string objectName, Stream data, long size, string contentType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 下载对象
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="objectName">对象键</param>
	/// <param name="callback">回调，形参为对象内容流，仅在回调执行期间可读</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task GetObjectAsync(string bucket, string objectName, Action<Stream> callback,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 删除对象
	/// </summary>
	/// <param name="bucket">桶名</param>
	/// <param name="objectName">对象键</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task RemoveObjectAsync(string bucket, string objectName, CancellationToken cancellationToken = default);
}
