using System.Text;
using Autofac;
using Autofac.Annotation;
using Autofac.Extensions.DependencyInjection;
using dotenv.net;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;
using Scalar.AspNetCore;
using sevencat.ruoyi.web.proxy;

namespace sevencat.ruoyi;

public class Program
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	public static void Main(string[] args)
	{
		DotEnv.Load();
		var builder = WebApplication.CreateBuilder(args);
		builder.Logging.ClearProviders().AddNLog();
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		var config = builder.Configuration;

		var afsp = new AutofacServiceProviderFactory(x => ConfigIoc(x, config));
		builder.Host.UseServiceProviderFactory(afsp);


		var mvcBuilder = builder.Services.AddControllers();
		mvcBuilder.AddControllersAsServices().AddJsonOptions((opts) =>
		{
			opts.JsonSerializerOptions.PropertyNamingPolicy = null;
			// 时间统一格式化为 "yyyy-MM-dd HH:mm:ss"
			opts.JsonSerializerOptions.Converters.Add(new common.json.JsonConverterUtil.DateTimeConverter());
			opts.JsonSerializerOptions.Converters.Add(new common.json.JsonConverterUtil.DateTimeNullConverter());
		});
		mvcBuilder.Services.AddHttpContextAccessor();

		// /api/** 兜底转发
		builder.Services.AddApiProxy(config);


		// builder.Services.AddSignalR().AddHubOptions<MsgHub>(options =>
		// {
		// 	options.DisableImplicitFromServicesParameters = true;
		// 	options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
		// });
		builder.WebHost.ConfigureKestrel(options =>
		{
			options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
		});
		builder.Services.AddOpenApi();

		// 跨域处理：默认允许任意来源（可按需改成指定域名）
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", policy =>
			{
				policy.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader();
			});
		});

		builder.Services.AddHangfire(configuration => configuration
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseMemoryStorage());
		builder.Services.AddHangfireServer(options =>
		{
			options.SchedulePollingInterval = TimeSpan.FromSeconds(2);
			options.HeartbeatInterval = TimeSpan.FromSeconds(2);
		});


		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
		app.UseRouting();

		app.UseCors("AllowAll");
		app.MapControllers();
		// 未被本项目处理的 /api/** 请求转发到目标地址
		app.MapApiProxy(config);
		app.MapHangfireDashboard();

		app.MapOpenApi();
		app.MapScalarApiReference();
		app.MapGet("/", () => Results.Redirect("/index.html"));
		{
			var exeDir = AppDomain.CurrentDomain.BaseDirectory;
			var wwwdir = Path.Combine(exeDir, "www");
			Directory.CreateDirectory(wwwdir);
			Log.Info("静态文件路径为:{0}", wwwdir);
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(wwwdir),
				RequestPath = "",
			});
		}
		app.Run();
	}

	private static void ConfigIoc(ContainerBuilder builder, IConfiguration config)
	{
		var module = new AutofacAnnotationModule(typeof(Program).Assembly);
		module.SetDefaultAutofacScopeToSingleInstance();
		module.SetDefaultValueResource(config);
		builder.RegisterModule(module);
	}
}