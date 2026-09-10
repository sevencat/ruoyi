using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/system/config")]
public class SysConfigController(SysConfigService configService)
{
	[SaCheckPermission("system:config:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysConfigVo>>> List([FromQuery] SysConfigBo config,
		[FromQuery] PageQuery2 pageQuery)
	{
		var itemlst = await configService.SelectPageConfigList(config, pageQuery);
		return itemlst.ToCommonResult();
	}

	[HttpGet("configKey/{configKey}")]
	public async Task<CommonResult<string>> getConfigKey([FromRoute] string configKey)
	{
		var item = await configService.SelectqConfigByKey(configKey);
		return item.ToCommonResult();
	}
}