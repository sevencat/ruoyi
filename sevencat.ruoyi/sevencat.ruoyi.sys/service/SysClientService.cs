using Autofac.Annotation;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysClientService(IFreeSql fsql)
{
	public async Task<SysClientVo> QueryById(long id)
	{
		return null;
	}
}