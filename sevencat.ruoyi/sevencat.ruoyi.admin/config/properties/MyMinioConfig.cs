using Autofac.Annotation;

namespace sevencat.ruoyi.config.properties;

[Component]
public class MyMinioConfig
{
	[Value("minio:endpoint")]
	public string EndPoint { get; set; }

	[Value("minio:url")]
	public string Url { get; set; }

	[Value("minio:isssl")]
	public bool IsSsl { get; set; }

	[Value("minio:realip")]
	public string RealIp { get; set; }

	[Value("minio:user")]
	public string User { get; set; }

	[Value("minio:pwd")]
	public string Pwd { get; set; }
}