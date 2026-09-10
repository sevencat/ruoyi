using Microsoft.AspNetCore.Mvc;
using sevencat.common.entity;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.controller;

[ApiController]
[Route("/api/system/dict/data")]
public class SysDictDataController(IFreeSql fsql)
{
	/**
     * 根据字典类型查询字典数据信息
     *
     * @param dictType 字典类型
     * @return 字典数据列表
     */
	[HttpGet("type/{dictType}")]
	public async Task<CommonResult<List<SysDictDataVo>>> dictType([FromRoute] string dictType)
	{
		var data = await fsql.Select<TSysDictData>()
			.Where(x => x.DictType == dictType)
			.ToListAsync();

		return data.MapTo<List<SysDictDataVo>>().ToCommonResult();
	}
}