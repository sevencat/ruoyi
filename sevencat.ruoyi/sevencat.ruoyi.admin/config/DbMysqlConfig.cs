using Autofac.Annotation;
using Autofac.Annotation.Condition;
using FreeSql;
using MySqlConnector;
using sevencat.common;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class DbMysqlConfig
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	[Bean]
	[ConditionalOnProperty("db:type", "mysql")]
	public IFreeSql CreateMysql(IConfiguration cfg)
	{
		var dburl = cfg.GetValue<string>("db:url");
		if (dburl.IsNullOrWhiteSpace())
		{
			throw new Exception("db:url未设置mysql连接参数");
		}

		var blder = new MySqlConnectionStringBuilder(dburl)
		{
			AllowZeroDateTime = true,
			OldGuids = true,
			CharacterSet = "utf8",
			Pooling = true,
			IgnoreCommandTransaction = true,
		};
		var connectionString = blder.ToString();

		var fsql = new FreeSqlBuilder()
			.UseConnectionString(DataType.MySql, connectionString)
			.UseNoneCommandParameter(true)
			.UseAutoSyncStructure(false)
			.Build();

		var usejsonmap = cfg.GetValue<int>("db:usejsonmap");
		if (usejsonmap == 1)
			fsql.UseJsonMap();
		var showsql = cfg.GetValue<int>("db:showsql");
		if (showsql == 1)
			fsql.Aop.CurdBefore += (_, e) => { Log.Info(e.Sql); };
		return fsql;
	}
}