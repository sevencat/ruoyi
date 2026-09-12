using Autofac.Annotation;
using sevencat.common;

namespace sevencat.ruoyi.config.properties;

/// <summary>
/// 对象存储配置（appsettings.json 的 minio 节），MinIO 实现与本地文件实现共用
/// </summary>
[Component]
public class MyOssConfig
{
	/// <summary>
	/// 缺省桶名（未配置 <c>minio:bucket</c> 时使用）
	/// </summary>
	private const string DefaultBucket = "ruoyi";

	public const int OssTypeLocal = 1;

	public const int OssTypeMinio = 2;
	

	//1-本地，2-minio
	[Value("oss:osstype")]
	public int OssType { get; set; }

	[Value("oss:endpoint")]
	public string EndPoint { get; set; }

	/// <summary>
	/// 对象的对外访问地址前缀（可带路径前缀，如 http://127.0.0.1:7050/api/minio）
	/// </summary>
	[Value("oss:url")]
	public string Url { get; set; }

	/// <summary>
	/// 桶名
	/// </summary>
	[Value("oss:bucket")]
	public string Bucket { get; set; }

	/// <summary>
	/// 本地存储根目录（仅本地文件实现使用），未配置时为 <c>{程序目录}/minio-data</c>
	/// </summary>
	[Value("oss:root")]
	public string Root { get; set; }

	[Value("oss:isssl")]
	public bool IsSsl { get; set; }

	[Value("oss:realip")]
	public string RealIp { get; set; }

	[Value("oss:user")]
	public string User { get; set; }

	[Value("oss:pwd")]
	public string Pwd { get; set; }

	/// <summary>
	/// 实际使用的桶名，未配置时回退为缺省桶
	/// </summary>
	public string EffectiveBucket => Bucket.IsNotNullOrWhiteSpace() ? Bucket : DefaultBucket;

	/// <summary>
	/// 本地存储根目录绝对路径，未配置时回退为 <c>{程序目录}/minio-data</c>
	/// </summary>
	/// <remarks>与 <c>MinioMockController</c> 的目录约定保持一致（同名配置项 <c>minio:root</c>）</remarks>
	public string EffectiveRoot => Root.IsNotNullOrWhiteSpace()
		? Path.GetFullPath(Root)
		: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minio-data");
}