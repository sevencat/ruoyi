using System.Reflection;
using Autofac.Annotation;
using Mapster;
using MapsterMapper;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class MapperConfig
{
	[Bean]
	public IMapper CreateMapper()
	{
		var config = TypeAdapterConfig.GlobalSettings;
		// 扫描当前程序集中的所有实现 IRegister 的配置类（选填，用于解耦配置）
		config.Scan(Assembly.GetExecutingAssembly());
		return new Mapper();
	}
}