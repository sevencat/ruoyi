using MapsterMapper;

namespace sevencat.ruoyi.common.util;

public static class MapperExt
{
	public static TDestination MapTo<TDestination>(this object source, IMapper mapper)
	{
		return mapper.Map<TDestination>(source);
	}
}