using System.Net;
using System.Net.Sockets;
using Autofac.Annotation;
using Minio;
using sevencat.common;
using sevencat.ruoyi.common.oss;
using sevencat.ruoyi.config.impl;
using sevencat.ruoyi.config.properties;

namespace sevencat.ruoyi.config;

//TODO:如果需要对接到真实的minio,把下面返回的 LocalOssClient 换成 MinioOssClient（两个实现都已注册为组件）
//      [Bean] public IOssClient CreateOssClient(MinioOssClient mc) => mc;
[AutoConfiguration]
public class OssConfig
{
	[Bean]
	public IOssClient CreateOssClient(MyOssConfig miconfig)
	{
		if (miconfig.OssType == 1)
		{
			return new LocalOssClient(miconfig);
		}
		else
		{
			var minioc = CreateMinio(miconfig);
			return new MinioOssClient(minioc, miconfig);
		}
	}

	private HttpClient CreateMinioHc(MyOssConfig miconfig)
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

	public IMinioClient CreateMinio(MyOssConfig miconfig)
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