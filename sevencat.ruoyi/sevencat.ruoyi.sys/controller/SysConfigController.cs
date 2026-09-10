using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.core.entity;
using sevencat.ruoyi.core.security.attr;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.service;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/system/config")]
public class SysConfigController(SysConfigService sysConfigService)
{
	[SaCheckPermission("system:config:list")]
	[HttpGet("list")]
	public async Task<CommonResult<PageResult<SysConfigVo>>> List([FromQuery] SysConfigBo config,
		[FromQuery] PageQuery pageQuery)
	{
		var itemlst = await sysConfigService.SelectPageConfigList(config, pageQuery);
		return itemlst.ToCommonResult();
	}
}