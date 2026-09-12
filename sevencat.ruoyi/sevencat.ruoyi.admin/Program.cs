using System.Text;
using Autofac;
using Autofac.Annotation;
using Autofac.Extensions.DependencyInjection;
using dotenv.net;
using NLog.Extensions.Logging;
using sevencat.ruoyi.common;
using sevencat.ruoyi.sys;

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

		var afsp = new AutofacServiceProviderFactory(x => ConfigIoc(x, config, builder.Environment));
		builder.Host.UseServiceProviderFactory(afsp);

		builder.WebHost.ConfigureKestrel(options =>
		{
			options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
		});


		var app = builder.Build();
		AppModule.SetupPipeline(app, config);
		app.Run();
	}

	private static void ConfigIoc(ContainerBuilder builder,
		IConfiguration config,
		IWebHostEnvironment env)
	{
		var module = new AutofacAnnotationModule(typeof(Program).Assembly,
			typeof(CommonModule).Assembly,
			typeof(SysModule).Assembly);
		module.SetDefaultAutofacScopeToSingleInstance();
		module.SetDefaultValueResource(config);
		builder.RegisterModule(module);
		builder.RegisterModule<CommonModule>();
		builder.RegisterModule<SysModule>();
		builder.RegisterModule(new AppModule(env));
	}
}