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
		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
		app.UseRouting();

		app.UseCors("AllowAll");
		app.MapControllers();

		app.MapHangfireDashboard("/hf", new DashboardOptions
		{
			Authorization = [new MyDashboardFilter()]
		});
		Log.Info("hangfire后缀为/hf");
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
	}
}