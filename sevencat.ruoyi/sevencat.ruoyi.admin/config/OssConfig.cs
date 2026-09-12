using System.Net;
using System.Net.Sockets;
using Autofac.Annotation;
using Minio;
using sevencat.common;
using sevencat.ruoyi.common.oss;
using sevencat.ruoyi.config.impl;
using sevencat.ruoyi.config.properties;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class OssConfig
{
	[Bean]
	public IOssClient CreateOssClient(LocalOssClient oc)
	{
		return oc;
	}

	private HttpClient CreateMinioHc(MyMinioConfig miconfig)
	{
		if (miconfig.RealIp.IsNullOrWhiteSpace())
			return new HttpClient()
			{
				BaseAddress = new Uri(miconfig.Url),
			};
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

	public IMinioClient CreateMinio(MyMinioConfig miconfig)
	{
		var customHttpClient = CreateMinioHc(miconfig);
		var minio = new MinioClient()
			.WithEndpoint(miconfig.EndPoint)
			.WithCredentials(miconfig.User, miconfig.Pwd)
			.WithSSL(false)
			.WithHttpClient(customHttpClient)
			.Build();
		return minio;
	}
}