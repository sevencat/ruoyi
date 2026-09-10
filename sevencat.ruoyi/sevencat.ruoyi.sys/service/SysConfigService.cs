using Autofac.Annotation;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysConfigService(IFreeSql fsql, IMapper mapper)
{
	/**
     * 分页查询参数配置列表
     *
     * @param config    查询条件
     * @param pageQuery 分页参数
     * @return 参数配置分页列表
     */
	public async Task<PageResult<SysConfigVo>> SelectPageConfigList(SysConfigBo config, PageQuery2 pageQuery)
	{
		var q = fsql.Select<TSysConfig>()
			.WhereIf(config.ConfigName.IsNotNullOrWhiteSpace(), x => x.ConfigName.Contains(config.ConfigName))
			.WhereIf(config.ConfigType.IsNotNullOrWhiteSpace(), x => x.ConfigType == config.ConfigType)
			.WhereIf(config.ConfigKey.IsNotNullOrWhiteSpace(), x => x.ConfigKey.Contains(config.ConfigKey))
			.OrderBy(x => x.ConfigId);
		var itemlst = await q.ToPage(pageQuery);
		return itemlst.MapTo<SysConfigVo>(mapper);
	}

	public async Task<string> SelectqConfigByKey(string configKey)
	{
		return await fsql.Select<TSysConfig>()
			.Where(x => x.ConfigKey == configKey)
			.FirstAsync(x => x.ConfigValue);
	}
}