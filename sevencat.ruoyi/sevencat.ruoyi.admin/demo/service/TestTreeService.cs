using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.demo.bo;
using sevencat.ruoyi.demo.entity.db;
using sevencat.ruoyi.demo.vo;

namespace sevencat.ruoyi.demo.service;

/// <summary>
/// 测试树表业务层（对应 Java 的 <c>ITestTreeService</c> / <c>TestTreeServiceImpl</c>）
/// </summary>
[Component]
public class TestTreeService(IFreeSql fsql, IMapper mapper)
{
	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const int DEL_FLAG_NORMAL = 0;

	/// <summary>
	/// 已删除标志，对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const int DEL_FLAG_DELETED = 1;

	/// <summary>
	/// 根据主键查询测试树表详情（对应 Java 的 <c>queryById</c>）
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>测试树表视图对象；不存在时返回 null</returns>
	public async Task<TestTreeVo> QueryByIdAsync(long id)
	{
		var row = await fsql.Select<TTestTree>()
			.Where(x => x.DelFlag == DEL_FLAG_NORMAL)
			.Where(x => x.Id == id)
			.FirstAsync();
		return row?.MapTo<TestTreeVo>(mapper);
	}

	/// <summary>
	/// 查询符合条件的测试树表列表（对应 Java 的 <c>queryList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>结果列表</returns>
	public async Task<List<TestTreeVo>> QueryListAsync(TestTreeBo bo)
	{
		var rows = await BuildTreeQuery(bo).ToListAsync();
		return rows.MapTo<List<TestTreeVo>>(mapper);
	}

	/// <summary>
	/// 根据新增业务对象插入测试树表（对应 Java 的 <c>insertByBo</c>）
	/// </summary>
	/// <param name="bo">测试树表新增业务对象</param>
	/// <returns>是否新增成功</returns>
	public async Task<bool> InsertByBoAsync(TestTreeBo bo)
	{
		var entity = bo.MapTo<TTestTree>(mapper);
		ValidEntityBeforeSave(entity);

		// 创建人 / 创建部门 / 创建时间以及主键雪花ID由 FSqlAop 自动填充
		var rows = await fsql.Insert(entity).ExecuteAffrowsAsync();
		if (rows > 0)
		{
			// 回写主键，行为对齐 Java 的 bo.setId(add.getId())
			bo.Id = entity.Id;
		}

		return rows > 0;
	}

	/// <summary>
	/// 根据编辑业务对象修改测试树表（对应 Java 的 <c>updateByBo</c>）
	/// </summary>
	/// <param name="bo">测试树表编辑业务对象</param>
	/// <returns>是否修改成功</returns>
	public async Task<bool> UpdateByBoAsync(TestTreeBo bo)
	{
		var entity = bo.MapTo<TTestTree>(mapper);
		ValidEntityBeforeSave(entity);

		// Java 原注解 @Version：C# 端无乐观锁插件，退化为「传入版本号 +1」写入以保持字段语义
		if (entity.Version.HasValue)
		{
			entity.Version += 1;
		}

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		var rows = await fsql.Update<TTestTree>()
			.SetSourceIgnore(entity)
			.Where(x => x.Id == entity.Id)
			.ExecuteAffrowsAsync();
		return rows > 0;
	}

	/// <summary>
	/// 校验并删除数据（对应 Java 的 <c>deleteWithValidByIds</c>）
	/// </summary>
	/// <param name="ids">主键集合</param>
	/// <param name="isValid">是否校验，true-删除前校验，false-不校验</param>
	/// <returns>是否删除成功</returns>
	public async Task<bool> DeleteWithValidByIdsAsync(List<long> ids, bool isValid)
	{
		if (ids == null || ids.Count == 0)
		{
			return false;
		}

		if (isValid)
		{
			// TODO 做一些业务上的校验，判断是否需要校验
		}

		// 对应 Java 的 @TableLogic 逻辑删除：deleteByIds 实际是 update del_flag = 1
		return await fsql.Update<TTestTree>()
			.Set(x => x.DelFlag, DEL_FLAG_DELETED)
			.Where(x => ids.Contains(x.Id))
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 构造测试树表列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">测试树表筛选条件</param>
	/// <returns>测试树表列表查询对象</returns>
	private ISelect<TTestTree> BuildTreeQuery(TestTreeBo bo)
	{
		return fsql.Select<TTestTree>()
			// 对应 Java 实体上的 @TableLogic：查询自动追加 del_flag = 0
			.Where(x => x.DelFlag == DEL_FLAG_NORMAL)
			.WhereNotNullEq(bo.DeptId, x => x.DeptId)
			.WhereNotNullEq(bo.UserId, x => x.UserId)
			.WhereLike(bo.TreeName, x => x.TreeName)
			// 对应 buildQueryWrapper 中的 orderByAsc(TestTree::getId)
			.OrderBy(x => x.Id);
	}

	/// <summary>
	/// 保存前的数据校验（对应 Java 的 <c>validEntityBeforeSave</c>）
	/// </summary>
	/// <param name="entity">实体类数据</param>
	private static void ValidEntityBeforeSave(TTestTree entity)
	{
		// TODO 做一些数据校验，如唯一约束
	}
}
