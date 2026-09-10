using System.Data.SQLite;
using Autofac.Annotation;
using Autofac.Annotation.Condition;
using sevencat.common;

namespace sevencat.ruoyi.config;

public class DbSqliteConfig
{
	private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

	//一个是长名字，一个是短名字
	[Bean]
	[ConditionalOnProperty("db:type", "sqlite")]
	public IFreeSql CreateSqliteDb(IConfiguration cfg)
	{
		var dbfn = cfg.GetValue<string>("db:fullfn");
		if (dbfn.IsNullOrWhiteSpace())
		{
			var shortfn = cfg.GetValue<string>("db:shortfn");
			//如果长名字没有设置
			var exedir = AppDomain.CurrentDomain.BaseDirectory;
			dbfn = Path.Combine(exedir, shortfn + ".db");
			Log.Info("数据库文件为:{0}", dbfn);
			if (!File.Exists(dbfn))
				SQLiteConnection.CreateFile(dbfn);
		}

		var dbconnbuilder = new SQLiteConnectionStringBuilder();
		dbconnbuilder.DataSource = dbfn;
		dbconnbuilder.JournalMode = SQLiteJournalModeEnum.Wal;

		var busytimeout = cfg.GetValue<int>("db:busytimeout");
		if (busytimeout <= 0)
			busytimeout = 8000;
		dbconnbuilder.BusyTimeout = busytimeout; //8秒
		dbconnbuilder.SyncMode = SynchronizationModes.Normal;
		dbconnbuilder.CacheSize = -32000;
		dbconnbuilder.Pooling = true;

		var fsql = new FreeSql.FreeSqlBuilder()
			.UseConnectionString(FreeSql.DataType.Sqlite, dbconnbuilder.ToString())
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