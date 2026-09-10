using Autofac.Annotation;
using sevencat.common.entity;
using sevencat.ruoyi.core.entity;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysConfigService(IFreeSql fsql)
{
	/**
     * 分页查询参数配置列表
     *
     * @param config    查询条件
     * @param pageQuery 分页参数
     * @return 参数配置分页列表
     */
	public async Task<PageResult<SysConfigVo>> SelectPageConfigList(SysConfigBo config, PageQuery pageQuery)
	{
	}
}