using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Annotation;
using Autofac.Extensions.DependencyInjection;
using dotenv.net;
using NLog.Extensions.Logging;
using sevencat.ruoyi.common;
using sevencat.ruoyi.sys;
using sevencat.ruoyi.sys.util;

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

		var mvcBuilder = builder.Services.AddControllers(options =>
		{
			// 全局注册操作日志切面（对应 Java 的 LogAspect），仅对打了 [Log] 特性的 action 生效
			options.Filters.Add<OperLogFilter>();
		});
		mvcBuilder.AddControllersAsServices().AddJsonOptions((opts) =>
		{
			opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
			opts.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
			// 时间统一格式化为 "yyyy-MM-dd HH:mm:ss"
			opts.JsonSerializerOptions.Converters.Add(new sevencat.common.json.JsonConverterUtil.DateTimeConverter());
			opts.JsonSerializerOptions.Converters.Add(
				new sevencat.common.json.JsonConverterUtil.DateTimeNullConverter());
		});
		builder.WebHost.ConfigureKestrel(options =>
		{
			options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
		});


		var app = builder.Build();
		AppModule.SetupPipeline(app, config);
		app.Run();
	}

	private static void ConfigIoc(ContainerBuilder builder, IConfiguration config)
	{
		var module = new AutofacAnnotationModule(typeof(Program).Assembly,
			typeof(CommonModule).Assembly,
			typeof(SysModule).Assembly);
		module.SetDefaultAutofacScopeToSingleInstance();
		module.SetDefaultValueResource(config);
		builder.RegisterModule(module);
		builder.RegisterModule<CommonModule>();
		builder.RegisterModule<SysModule>();
		builder.RegisterModule(new AppModule(config));
	}
}