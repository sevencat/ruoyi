using Autofac.Util;
using MapsterMapper;

namespace sevencat.ruoyi.common.util;

public static class MapperExt
{
	private static readonly Lazy<IMapper> _mapper = IocFactory.CreateLazy<IMapper>();

	public static TDestination MapTo<TDestination>(this object source, IMapper mapper)
	{
		return mapper.Map<TDestination>(source);
	}

	public static TDestination MapTo<TDestination>(this object source)
	{
		return _mapper.Value.Map<TDestination>(source);
	}
	
	
}