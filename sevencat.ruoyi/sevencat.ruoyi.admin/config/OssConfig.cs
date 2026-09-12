using System.Net;
using System.Net.Sockets;
using Autofac.Annotation;
using Minio;
using sevencat.common;
using sevencat.ruoyi.config.properties;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class OssConfig
{
	private HttpClient CreateMinioHc(MyMinioConfig miconfig)
	{
		if (miconfig.RealIp.IsNullOrWhiteSpace())
			return new HttpClient();
		var socketsHandler = new SocketsHttpHandler
		{
			ConnectCallback = async (context, cancellationToken) =>
			{
				var ipAddress = IPAddress.Parse(miconfig.RealIp);
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await socket.ConnectAsync(new IPEndPoint(ipAddress, context.DnsEndPoint.Port), cancellationToken);
				return new NetworkStream(socket, ownsSocket: true);
			}
		};
		return new HttpClient(socketsHandler);
	}

	[Bean]
	public IMinioClient CreateMinio(MyMinioConfig miconfig)
	{
		var customHttpClient = CreateMinioHc(miconfig);
		var minio = new MinioClient()
			.WithEndpoint(miconfig.Url)
			.WithCredentials(miconfig.User, miconfig.Pwd)
			.WithSSL(false)
			.WithHttpClient(customHttpClient)
			.Build();
		return minio;
	}
}