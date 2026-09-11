using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.security;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 岗位业务层（对应 Java 的 <c>ISysPostService</c> / <c>SysPostServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 接口中 <c>selectPostAll</c> / <c>selectPostsByUserId</c> / <c>selectPostNamesByIds</c> /
/// <c>deletePostById</c> 属于其他模块的调用入口，本项目暂无调用方，未实现。
/// Java 中依赖数据权限的 <c>checkPostDataScope</c> / <c>selectPostCount</c> 需要
/// <c>DataPermissionHelper</c> 支撑，C# 端暂无该设施，故未实现。
/// Java 端 VO 上的 <c>@Translation(type = TransConstant.DEPT_ID_TO_NAME, mapper = "deptId")</c>
/// 由翻译插件自动回填 <c>deptName</c>，C# 端无翻译注解，改为查询后按 <c>DeptId</c> 手工回填。
/// </remarks>
[Component]
public class SysPostService(IFreeSql fsql, IMapper mapper, SysDeptService deptService)
{
	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const string DEL_FLAG_DELETED = "1";

	/// <summary>
	/// 分页查询岗位列表（对应 Java 的 <c>selectPagePostList</c>）
	/// </summary>
	/// <param name="post">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>岗位分页列表（已回填部门名称）</returns>
	public async Task<PageResult<SysPostVo>> SelectPagePostList(SysPostBo post, PageQuery2 pageQuery)
	{
		var page = await (await BuildPostQuery(post)).ToPage(pageQuery);
		var result = page.MapTo<SysPostVo>(mapper);
		await FillDeptName(result.Rows);
		return result;
	}

	/// <summary>
	/// 根据条件查询岗位列表（对应 Java 的 <c>selectPostList</c>）
	/// </summary>
	/// <param name="post">岗位筛选条件</param>
	/// <returns>岗位列表（已回填部门名称）</returns>
	public async Task<List<SysPostVo>> SelectPostList(SysPostBo post)
	{
		var rows = await (await BuildPostQuery(post)).ToListAsync();
		var list = rows.MapTo<List<SysPostVo>>(mapper);
		await FillDeptName(list);
		return list;
	}

	/// <summary>
	/// 通过岗位ID查询岗位信息（对应 Java 的 <c>selectPostById</c>）
	/// </summary>
	/// <param name="postId">岗位ID</param>
	/// <returns>岗位信息（已回填部门名称）；不存在时返回 null</returns>
	public async Task<SysPostVo> SelectPostById(long postId)
	{
		var row = await fsql.Select<TSysPost>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.PostId == postId)
			.FirstAsync();
		if (row == null)
		{
			return null;
		}

		var vo = row.MapTo<SysPostVo>(mapper);
		await FillDeptName([vo]);
		return vo;
	}

	/// <summary>
	/// 通过岗位ID串查询岗位（对应 Java 的 <c>selectPostByIds</c>）
	/// </summary>
	/// <param name="postIds">岗位ID集合</param>
	/// <returns>岗位列表信息（只含岗位ID、名称、编码）</returns>
	public async Task<List<SysPostVo>> SelectPostByIds(List<long> postIds)
	{
		// 对应 Java 的 CollUtil.isNotEmpty(postIds)，为空时不追加 in 条件
		postIds ??= [];

		// 对应 Java 的 select：(postId, postName, postCode)，status = '0'，postId 判空匹配
		var rows = await fsql.Select<TSysPost>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.Status == SystemConstants.NORMAL)
			.WhereIf(postIds.Count > 0, x => postIds.Contains(x.PostId))
			.ToListAsync(x => new { x.PostId, x.PostName, x.PostCode });

		return rows.Select(row => new SysPostVo
		{
			PostId = row.PostId,
			PostName = row.PostName,
			PostCode = row.PostCode
		}).ToList();
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

	/// <summary>
	/// 校验岗位名称是否唯一（对应 Java 的 <c>checkPostNameUnique</c>）
	/// </summary>
	/// <param name="post">岗位信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckPostNameUnique(SysPostBo post)
	{
		// Java 的 eq(SysPost::getDeptId, post.getDeptId())：部门为空时恒不成立
		var q = fsql.Select<TSysPost>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.PostName == post.PostName);
		q = post.DeptId.HasValue
			? q.Where(x => x.DeptId == post.DeptId.Value)
			: q.Where("1 = 0");

		var exist = await q
			.WhereIf(post.PostId.HasValue, x => x.PostId != post.PostId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 校验岗位编码是否唯一（对应 Java 的 <c>checkPostCodeUnique</c>）
	/// </summary>
	/// <param name="post">岗位信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckPostCodeUnique(SysPostBo post)
	{
		var exist = await fsql.Select<TSysPost>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.PostCode == post.PostCode)
			.WhereIf(post.PostId.HasValue, x => x.PostId != post.PostId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 通过岗位ID查询岗位使用数量（对应 Java 的 <c>countUserPostById</c>）
	/// </summary>
	/// <param name="postId">岗位ID</param>
	/// <returns>岗位分配的用户数量</returns>
	public async Task<long> CountUserPostById(long postId)
	{
		// Java 的 SQL：select count(1) from sys_user_post where post_id = #{postId}
		return await fsql.Select<TSysUserPost>()
			.Where(x => x.PostId == postId)
			.CountAsync();
	}

	/// <summary>
	/// 根据部门ID查询岗位数量（对应 Java 的 <c>countPostByDeptId</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>岗位数量</returns>
	public async Task<long> CountPostByDeptId(long deptId)
	{
		// Java 的 SysPostMapper.countPostByDeptId：select count(1) from sys_post where dept_id = #{deptId}
		// 删除标志由 Java 的 @TableLogic 隐式追加，此处显式过滤
		return await fsql.Select<TSysPost>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.DeptId == deptId)
			.CountAsync();
	}

	/// <summary>
	/// 批量删除岗位信息（对应 Java 的 <c>deletePostByIds</c>）
	/// </summary>
	/// <param name="postIds">需要删除的岗位ID</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeletePostByIds(List<long> postIds)
	{
		if (postIds == null || postIds.Count == 0)
		{
			return 0;
		}

		var list = await fsql.Select<TSysPost>()
			.Where(x => postIds.Contains(x.PostId))
			.ToListAsync();
		foreach (var post in list)
		{
			if (await CountUserPostById(post.PostId) > 0)
			{
				throw new ServiceException($"{post.PostName}已分配，不能删除!");
			}
		}

		// 对应 Java 的 @TableLogic 逻辑删除：deleteByIds 实际是 update del_flag = '1'
		return await fsql.Update<TSysPost>()
			.Set(a => a.DelFlag, DEL_FLAG_DELETED)
			.Where(a => postIds.Contains(a.PostId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 新增保存岗位信息（对应 Java 的 <c>insertPost</c>）
	/// </summary>
	/// <param name="bo">岗位信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> InsertPost(SysPostBo bo)
	{
		var post = bo.MapTo<TSysPost>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		post.CreateBy ??= await LoginHelper.GetLoginUid();

		return await fsql.Insert(post).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 修改保存岗位信息（对应 Java 的 <c>updatePost</c>）
	/// </summary>
	/// <param name="bo">岗位信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdatePost(SysPostBo bo)
	{
		var post = bo.MapTo<TSysPost>(mapper);
		post.UpdateBy ??= await LoginHelper.GetLoginUid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		return await fsql.Update<TSysPost>()
			.SetSourceIgnore(post)
			.Where(a => a.PostId == post.PostId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 构造岗位列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">岗位筛选条件</param>
	/// <returns>岗位列表查询对象</returns>
	private async Task<ISelect<TSysPost>> BuildPostQuery(SysPostBo bo)
	{
		var q = fsql.Select<TSysPost>()
			// 对应 Java 实体字段上的 @TableLogic，MyBatis-Plus 会隐式追加 del_flag = '0'
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereLike(bo.PostCode, x => x.PostCode)
			.WhereLike(bo.PostCategory, x => x.PostCategory)
			.WhereLike(bo.PostName, x => x.PostName)
			.WhereHasTextEq(bo.Status, x => x.Status)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(bo.Params, x => x.CreateTime);

		if (bo.DeptId.HasValue)
		{
			// 优先单部门搜索
			q = q.Where(x => x.DeptId == bo.DeptId.Value);
		}
		else if (bo.BelongDeptId.HasValue)
		{
			// 部门树搜索（对应 Java 的 deptMapper.selectDeptAndChildById）
			var deptIds = await deptService.SelectDeptAndChildById(bo.BelongDeptId.Value);
			q = deptIds.Count > 0
				? q.Where(x => deptIds.Contains(x.DeptId))
				: q.Where("1 = 0");
		}

		return q.OrderBy(x => x.PostSort);
	}

	/// <summary>
	/// 回填岗位列表的部门名称（对应 Java 的 @Translation DEPT_ID_TO_NAME）
	/// </summary>
	/// <param name="rows">岗位列表</param>
	private async Task FillDeptName(List<SysPostVo> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		var deptIds = rows.Where(x => x.DeptId.HasValue)
			.Select(x => x.DeptId.Value)
			.Distinct()
			.ToList();
		if (deptIds.Count == 0)
		{
			return;
		}

		var depts = await fsql.Select<TSysDept>()
			.Where(x => deptIds.Contains(x.DeptId))
			.ToListAsync(x => new { x.DeptId, x.DeptName });
		var deptNames = depts.ToDictionary(x => x.DeptId, x => x.DeptName);

		foreach (var row in rows)
		{
			if (row.DeptId.HasValue && deptNames.TryGetValue(row.DeptId.Value, out var deptName))
			{
				row.DeptName = deptName;
			}
		}
	}
}
