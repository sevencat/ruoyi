using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace sevencat.ruoyi.web.proxy;

/// <summary>
/// 兜底反向代理配置：所有本项目未实现的 {PathPrefix}/** 请求都会被转发到 TargetAddress。
/// </summary>
public class ApiProxyOptions
{
	/// <summary>是否启用兜底转发。</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>目标地址，例如 http://127.0.0.1:8080（末尾斜杠会被忽略）。</summary>
	public string TargetAddress { get; set; } = "";

	/// <summary>需要拦截并在转发时去掉的路径前缀，默认 /api；置空表示不拦截也不去前缀。</summary>
	public string PathPrefix { get; set; } = "/api";

	/// <summary>转发请求超时时间（秒）。</summary>
	public int TimeoutSeconds { get; set; } = 100;
}

public static class ApiProxyExtensions
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 注册兜底转发所需的服务。可通过环境变量 ApiProxy__TargetAddress 覆盖配置。
	/// </summary>
	public static IServiceCollection AddApiProxy(this IServiceCollection services, IConfiguration config)
	{
		services.Configure<ApiProxyOptions>(config.GetSection("ApiProxy"));
		services.AddHttpForwarder();

		// 复用同一个 HttpClient，避免每个请求都新建连接池
		services.AddSingleton(_ =>
		{
			var handler = new SocketsHttpHandler
			{
				UseProxy = false,
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.None,
				UseCookies = false,
				ConnectTimeout = TimeSpan.FromSeconds(15),
				ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
			};
			return new HttpMessageInvoker(handler);
		});

		return services;
	}

	/// <summary>
	/// 映射兜底转发端点。路由优先级最低，因此只有未被控制器等具体端点匹配的请求才会走到这里。
	/// </summary>
	public static IEndpointRouteBuilder MapApiProxy(this IEndpointRouteBuilder endpoints, IConfiguration config)
	{
		var options = config.GetSection("ApiProxy").Get<ApiProxyOptions>() ?? new ApiProxyOptions();
		if (!options.Enabled)
		{
			Log.Info("ApiProxy 未启用，跳过兜底转发。");
			return endpoints;
		}

		if (string.IsNullOrWhiteSpace(options.TargetAddress))
		{
			Log.Warn("ApiProxy 已启用但未配置 TargetAddress，兜底转发不会生效。");
			return endpoints;
		}

		var prefix = string.IsNullOrWhiteSpace(options.PathPrefix) ? "" : "/" + options.PathPrefix.Trim().Trim('/');
		var target = options.TargetAddress.TrimEnd('/');
		var requestConfig = new ForwarderRequestConfig
		{
			ActivityTimeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
		};
		var transformer = new StripPrefixTransformer(prefix);

		Log.Info("已启用兜底转发 {0}/{1} -> {2}", prefix, "{**catch-all}", target);

		endpoints.Map($"{prefix}/{{**catch-all}}",
			async (HttpContext context, IHttpForwarder forwarder, HttpMessageInvoker invoker) =>
			{
				var error = await forwarder.SendAsync(context, target, invoker, requestConfig, transformer);
				if (error != ForwarderError.None)
				{
					var feature = context.GetForwarderErrorFeature();
					Log.Error(feature?.Exception, "转发 {0} 到 {1} 失败：{2}", context.Request.Path, target, error);
					if (!context.Response.HasStarted)
					{
						context.Response.StatusCode = StatusCodes.Status502BadGateway;
					}
				}
			});

		return endpoints;
	}

	/// <summary>
	/// 转发时去掉配置的路径前缀，例如 /api/hello?x=1 -> /hello?x=1。
	/// </summary>
	private sealed class StripPrefixTransformer : HttpTransformer
	{
		private readonly string _prefix;

		public StripPrefixTransformer(string prefix) => _prefix = prefix;

		public override ValueTask TransformRequestAsync(
			HttpContext httpContext,
			HttpRequestMessage proxyRequest,
			string destinationPrefix,
			CancellationToken cancellationToken)
		{
			var path = httpContext.Request.Path.Value ?? string.Empty;
			if (_prefix.Length > 0
			    && path.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase)
			    && (path.Length == _prefix.Length || path[_prefix.Length] == '/'))
			{
				path = path[_prefix.Length..];
			}

			if (path.Length == 0 || path[0] != '/')
			{
				path = "/" + path;
			}

			// 用 PathString/QueryString 的 ToUriComponent() 拼接，避免路径被重复转义。
			// 这里显式赋值 RequestUri，转发器不会再覆盖它，从而实现去掉前缀。
			var targetAddress = string.Concat(
				destinationPrefix,
				new PathString(path).ToUriComponent(),
				httpContext.Request.QueryString.ToUriComponent());

			proxyRequest.RequestUri = new Uri(targetAddress, UriKind.Absolute);
			return base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
		}
	}
}