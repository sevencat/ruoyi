using Autofac.Util;

namespace sevencat.ruoyi.common.db;

public class IdGenUtil
{
	private static readonly Lazy<IIdGen> _idgen = IocFactory.CreateLazy<IIdGen>();

	public static long NextId()
	{
		return _idgen.Value.NextId();
	}
}