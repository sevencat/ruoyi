using System.Text.Json;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using sevencat.ruoyi.config.impl;
using sevencat.ruoyi.sys.log;
using sevencat.ruoyi.web;

namespace sevencat.ruoyi;

public class AppModule(IWebHostEnvironment env) : Module
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	protected override void Load(ContainerBuilder builder)
	{
		var services = new ServiceCollection();
		ConfigServices(services);
		builder.Populate(services);
		base.Load(builder);
	}

	protected void ConfigServices(ServiceCollection Services)
	{
		//重要:如果不有这两行，AddControllers会失去效果!!!
		Services.AddSingleton<IWebHostEnvironment>(env);
		Services.AddSingleton<IHostEnvironment>(env);
		var mvcBuilder = Services.AddControllers();
		mvcBuilder.AddControllersAsServices().AddJsonOptions((opts) =>
		{
			//这个是缺省的web配置
			opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
			opts.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;

			//这个主要是雪花id是long,造成的
			opts.JsonSerializerOptions.Converters.Add(new LongToStringConverter());
			// 时间统一格式化为 "yyyy-MM-dd HH:mm:ss"5
			opts.JsonSerializerOptions.Converters.Add(new sevencat.common.json.JsonConverterUtil.DateTimeConverter());
			opts.JsonSerializerOptions.Converters.Add(
				new sevencat.common.json.JsonConverterUtil.DateTimeNullConverter());
		});

		Services.AddHttpContextAccessor();


		// builder.Services.AddSignalR().AddHubOptions<MsgHub>(options =>
		// {
		// 	options.DisableImplicitFromServicesParameters = true;
		// 	options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
		// });

		Services.AddOpenApi();

		// 跨域处理：默认允许任意来源（可按需改成指定域名）
		Services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", policy =>
			{
				policy.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader();
			});
		});

		Services.AddHangfire(configuration => configuration
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseMemoryStorage());
		Services.AddHangfireServer(options =>
		{
			options.SchedulePollingInterval = TimeSpan.FromSeconds(2);
			options.HeartbeatInterval = TimeSpan.FromSeconds(2);
		});

		// 操作日志写库后台线程：请求线程只把日志入队，落库在后台完成
		Services.AddHostedService<OperLogWorker>();
	}

	public static void SetupPipeline(WebApplication app, IConfiguration config)
	{
		app.UseMiddleware<GlobalExceptionMiddleware>();
		// 静态文件前置（放在 UseRouting 之前）：命中即短路管道，未命中则继续往下走路由。
		var exeDir = AppDomain.CurrentDomain.BaseDirectory;
		var wwwdir = Path.Combine(exeDir, "www");
		Directory.CreateDirectory(wwwdir);
		Log.Info("静态文件路径为:{0}", wwwdir);
		var wwwProvider = new PhysicalFileProvider(wwwdir);
		var wwwOptions = new StaticFileOptions
		{
			FileProvider = wwwProvider,
			RequestPath = "",
			OnPrepareResponse = ctx =>
			{
				// index.html 不能缓存，否则前端发版后仍会去取旧资源；其余资源给中等缓存期
				ctx.Context.Response.Headers.CacheControl =
					ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
						? "no-cache"
						: "public,max-age=604800";
			},
		};

		// 静态文件只对「非 /api 开头」的请求生效：/api/** 直接跳过文件探测，交给后面的路由。
		// 注意用 UseWhen（旁路分支，未短路时会汇合回主管道），不要用 MapWhen（终端分支，不会汇合，
		// 会让 /hf、/swagger 以及 SPA 回退这些非 /api 路径在未命中物理文件时直接 404）。
		// StartsWithSegments 是按路径段、忽略大小写匹配，不会把 /apixxx 误判成 /api。
		app.UseWhen(
			ctx => !ctx.Request.Path.StartsWithSegments("/api"),
			branch =>
			{
				// / 内部重写为 /index.html，省掉原来的 302 跳转
				branch.UseDefaultFiles(new DefaultFilesOptions { FileProvider = wwwProvider });
				branch.UseStaticFiles(wwwOptions);
			});

		app.UseRouting();

		app.UseCors("AllowAll");
		app.MapControllers();

		app.MapHangfireDashboard("/hf", new DashboardOptions
		{
			//如果需要认证，把这行注释去掉。
			//Authorization = [new MyDashboardFilter()]
		});
		Log.Info("hangfire后缀为/hf");
		app.MapOpenApi();
		app.MapScalarApiReference();
		// // 未知的 /api/** 保持 JSON 404，避免被下面的 SPA 回退吞成 200 HTML
		// app.MapMethods("/api/{**rest}", ["GET", "POST", "PUT", "DELETE", "PATCH"],
		// 	() => Results.Json(new { code = 404, msg = "Not Found" }, statusCode: StatusCodes.Status404NotFound));
		//
		// // SPA 回退：不存在的“非文件”路径（例如刷新 /system/user）返回首页；
		// // 带扩展名的路径（缺失的静态资源）仍返回 404，不会被吞成 HTML
		// app.MapFallbackToFile("{*path:nonfile}", "index.html", wwwOptions);
	}
}