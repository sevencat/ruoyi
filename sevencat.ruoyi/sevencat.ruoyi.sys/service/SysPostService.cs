using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 岗位业务层（对应 Java 的 <c>SysPostServiceImpl</c>）
/// </summary>
/// <remarks>
/// 目前只实现用户模块用到的部分。Java 中依赖数据权限的 <c>checkPostDataScope</c> / <c>selectPostCount</c>
/// 需要 <c>DataPermissionHelper</c> 支撑，C# 端暂无该设施，故未实现。
/// </remarks>
[Component]
public class SysPostService(IFreeSql fsql, IMapper mapper)
{
	/// <summary>
	/// 根据条件查询岗位列表（对应 Java 的 <c>selectPostList</c>）
	/// </summary>
	/// <param name="post">岗位筛选条件</param>
	/// <returns>岗位列表</returns>
	public async Task<List<SysPostVo>> SelectPostList(SysPostBo post)
	{
		// Java 的 selectPostList：p.status = '0' + 各条件判空匹配，按 dept_id、post_id 排序
		var q = fsql.Select<TSysPost>()
			.WhereHasTextEq(SystemConstants.NORMAL, x => x.Status)
			.WhereNotNullEq(post.PostId, x => x.PostId)
			.WhereHasTextEq(post.PostCode, x => x.PostCode)
			.WhereHasTextEq(post.Status, x => x.Status)
			.WhereLike(post.PostName, x => x.PostName)
			.WhereNotNullEq(post.DeptId, x => x.DeptId);

		var posts = await q.OrderBy(x => x.DeptId).OrderBy(x => x.PostId).ToListAsync();
		return posts.MapTo<List<SysPostVo>>(mapper);
	}

	/// <summary>
	/// 根据用户ID查询岗位ID列表（对应 Java 的 <c>selectPostListByUserId</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>岗位ID列表</returns>
	public async Task<List<long>> SelectPostListByUserId(long userId)
	{
		// Java 的 SQL：select post_id from sys_user_post where user_id = #{userId}
		return await fsql.Select<TSysUserPost>()
			.Where(x => x.UserId == userId)
			.ToListAsync(x => x.PostId);
	}
}
