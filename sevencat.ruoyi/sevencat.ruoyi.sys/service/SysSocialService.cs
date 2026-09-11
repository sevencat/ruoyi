using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 社会化关系业务层（对应 Java 的 <c>ISysSocialService</c> / <c>SysSocialServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 实体 <c>SysSocial</c> 未声明 <c>del_flag</c> / <c>@TableLogic</c>，
/// 因此查询不会隐式追加 <c>del_flag = '0'</c>，删除也是物理删除。
/// 主键与审计字段（create_by/create_time/update_by/update_time）由 <c>FSqlAop</c> 自动填充。
/// </remarks>
[Component]
public class SysSocialService(IFreeSql fsql, IMapper mapper)
{
	/// <summary>
	/// 查询社会化关系（对应 Java 的 <c>queryById</c>）
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>社会化绑定详情；不存在时返回 null</returns>
	public async Task<SysSocialVo> QueryById(long id)
	{
		var row = await fsql.Select<TSysSocial>()
			.Where(x => x.Id == id)
			.FirstAsync();

		return row?.MapTo<SysSocialVo>(mapper);
	}

	/// <summary>
	/// 授权列表（对应 Java 的 <c>queryList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>社会化授权关系列表</returns>
	public async Task<List<SysSocialVo>> QueryList(SysSocialBo bo)
	{
		var rows = await fsql.Select<TSysSocial>()
			.WhereNotNullEq(bo.UserId, x => x.UserId)
			.WhereHasTextEq(bo.AuthId, x => x.AuthId)
			.WhereHasTextEq(bo.Source, x => x.Source)
			.ToListAsync();

		return rows.MapTo<List<SysSocialVo>>(mapper);
	}

	/// <summary>
	/// 按用户主键查询其绑定的社会化授权列表（对应 Java 的 <c>queryListByUserId</c>）
	/// </summary>
	/// <param name="userId">用户主键</param>
	/// <returns>用户已绑定的社会化授权列表</returns>
	public async Task<List<SysSocialVo>> QueryListByUserId(long userId)
	{
		var rows = await fsql.Select<TSysSocial>()
			.Where(x => x.UserId == userId)
			.ToListAsync();

		return rows.MapTo<List<SysSocialVo>>(mapper);
	}

	/// <summary>
	/// 根据 authId 查询用户信息（对应 Java 的 <c>selectByAuthId</c>）
	/// </summary>
	/// <param name="authId">认证id</param>
	/// <returns>授权信息</returns>
	public async Task<List<SysSocialVo>> SelectByAuthId(string authId)
	{
		var rows = await fsql.Select<TSysSocial>()
			.Where(x => x.AuthId == authId)
			.ToListAsync();

		return rows.MapTo<List<SysSocialVo>>(mapper);
	}

	/// <summary>
	/// 新增社会化关系（对应 Java 的 <c>insertByBo</c>）
	/// </summary>
	/// <param name="bo">业务对象</param>
	/// <returns>新增成功返回 true</returns>
	public async Task<bool> InsertByBo(SysSocialBo bo)
	{
		var add = bo.MapTo<TSysSocial>(mapper);
		ValidEntityBeforeSave(add);

		var flag = await fsql.Insert(add).ExecuteAffrowsAsync() > 0;
		if (flag)
		{
			// 对应 Java 的 bo.setId(add.getId())，回填雪花主键供调用方使用
			bo.Id = add.Id;
		}

		return flag;
	}

	/// <summary>
	/// 更新社会化关系（对应 Java 的 <c>updateByBo</c>）
	/// </summary>
	/// <param name="bo">业务对象</param>
	/// <returns>更新成功返回 true</returns>
	public async Task<bool> UpdateByBo(SysSocialBo bo)
	{
		var update = bo.MapTo<TSysSocial>(mapper);
		ValidEntityBeforeSave(update);

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		return await fsql.Update<TSysSocial>()
			.SetSourceIgnore(update)
			.Where(a => a.Id == update.Id)
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 删除社会化关系（对应 Java 的 <c>deleteWithValidById</c>）
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>删除成功返回 true</returns>
	public async Task<bool> DeleteWithValidById(long id)
	{
		// Java 实体未使用 @TableLogic，deleteById 为物理删除
		return await fsql.Delete<TSysSocial>()
			.Where(x => x.Id == id)
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 保存前的数据校验（对应 Java 的 <c>validEntityBeforeSave</c>）
	/// </summary>
	/// <param name="entity">待保存的社会化关系实体</param>
	private static void ValidEntityBeforeSave(TSysSocial entity)
	{
		// Java 端此处为 TODO（如唯一约束校验），保持一致暂不实现
	}
}
