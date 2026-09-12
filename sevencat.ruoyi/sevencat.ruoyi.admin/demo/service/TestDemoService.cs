using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.demo.bo;
using sevencat.ruoyi.demo.entity.db;
using sevencat.ruoyi.demo.vo;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.demo.service;

/// <summary>
/// 测试单表业务层（对应 Java 的 <c>ITestDemoService</c> / <c>TestDemoServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端 VO 上的 <c>@Translation(type = TransConstant.USER_ID_TO_NAME, mapper = "createBy")</c>
/// 由翻译插件自动回填 <c>createByName</c> / <c>updateByName</c>，C# 端无翻译注解，
/// 改为查询后按 <c>CreateBy</c> / <c>UpdateBy</c> 手工回填。
/// </remarks>
[Component]
public class TestDemoService(IFreeSql fsql, IMapper mapper)
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
	/// 根据主键查询测试单表详情（对应 Java 的 <c>queryById</c>）
	/// </summary>
	/// <param name="id">主键</param>
	/// <returns>测试单表视图对象；不存在时返回 null</returns>
	public async Task<TestDemoVo> QueryByIdAsync(long id)
	{
		var row = await fsql.Select<TTestDemo>()
			.Where(x => x.DelFlag == DEL_FLAG_NORMAL)
			.Where(x => x.Id == id)
			.FirstAsync();
		if (row == null)
		{
			return null;
		}

		var vo = row.MapTo<TestDemoVo>(mapper);
		await FillUserName([vo]);
		return vo;
	}

	/// <summary>
	/// 分页查询测试单表列表（对应 Java 的 <c>queryPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>分页结果（已回填创建人/更新人账号）</returns>
	public async Task<PageResult<TestDemoVo>> QueryPageListAsync(TestDemoBo bo, PageQuery2 pageQuery)
	{
		var page = await BuildDemoQuery(bo).ToPage(pageQuery);
		var result = page.MapTo<TestDemoVo>(mapper);
		await FillUserName(result.Rows);
		return result;
	}

	/// <summary>
	/// 按自定义 SQL 分页查询测试单表列表（对应 Java 的 <c>customPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>分页结果（已回填创建人/更新人账号）</returns>
	/// <remarks>
	/// Java 端该方法的自定义 SQL 为 <c>SELECT * FROM test_demo ${ew.customSqlSegment}</c>，
	/// 与默认分页查询生成的 SQL 完全一致，C# 端直接复用同一段查询逻辑。
	/// </remarks>
	public async Task<PageResult<TestDemoVo>> CustomPageListAsync(TestDemoBo bo, PageQuery2 pageQuery)
	{
		return await QueryPageListAsync(bo, pageQuery);
	}

	/// <summary>
	/// 查询符合条件的测试单表列表（对应 Java 的 <c>queryList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <returns>结果列表（已回填创建人/更新人账号）</returns>
	public async Task<List<TestDemoVo>> QueryListAsync(TestDemoBo bo)
	{
		var rows = await BuildDemoQuery(bo).ToListAsync();
		var list = rows.MapTo<List<TestDemoVo>>(mapper);
		await FillUserName(list);
		return list;
	}

	/// <summary>
	/// 根据新增业务对象插入测试单表（对应 Java 的 <c>insertByBo</c>）
	/// </summary>
	/// <param name="bo">测试单表新增业务对象</param>
	/// <returns>是否新增成功</returns>
	public async Task<bool> InsertByBoAsync(TestDemoBo bo)
	{
		var entity = bo.MapTo<TTestDemo>(mapper);
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
	/// 根据编辑业务对象修改测试单表（对应 Java 的 <c>updateByBo</c>）
	/// </summary>
	/// <param name="bo">测试单表编辑业务对象</param>
	/// <returns>是否修改成功</returns>
	public async Task<bool> UpdateByBoAsync(TestDemoBo bo)
	{
		var entity = bo.MapTo<TTestDemo>(mapper);
		ValidEntityBeforeSave(entity);

		// Java 原注解 @Version：C# 端无乐观锁插件，退化为「传入版本号 +1」写入以保持字段语义
		if (entity.Version.HasValue)
		{
			entity.Version += 1;
		}

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		var rows = await fsql.Update<TTestDemo>()
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
			// 做一些业务上的校验，判断是否需要校验
			var list = await fsql.Select<TTestDemo>()
				.Where(x => x.DelFlag == DEL_FLAG_NORMAL)
				.Where(x => ids.Contains(x.Id))
				.ToListAsync();
			if (list.Count != ids.Count)
			{
				throw new ServiceException("您没有删除权限!");
			}
		}

		// 对应 Java 的 @TableLogic 逻辑删除：deleteByIds 实际是 update del_flag = 1
		return await fsql.Update<TTestDemo>()
			.Set(x => x.DelFlag, DEL_FLAG_DELETED)
			.Where(x => ids.Contains(x.Id))
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 批量保存测试单表数据（对应 Java 的 <c>saveBatch</c>）
	/// </summary>
	/// <param name="list">待保存数据</param>
	/// <returns>是否保存成功</returns>
	public async Task<bool> SaveBatchAsync(List<TTestDemo> list)
	{
		if (list == null || list.Count == 0)
		{
			return false;
		}

		return await fsql.Insert(list).ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// Excel 导入数据（对应 Java 的 <c>importData</c> 中 ExcelBuilder 读取 + saveBatch 的组合）
	/// </summary>
	/// <param name="list">Excel 解析出的导入行</param>
	/// <returns>导入结果说明；存在非法数据时抛出 <see cref="ServiceException"/></returns>
	public async Task<string> ImportAsync(List<TestDemoImportVo> list)
	{
		if (list == null || list.Count == 0)
		{
			return "没有导入数据";
		}

		var entities = new List<TTestDemo>(list.Count);
		for (var i = 0; i < list.Count; i++)
		{
			var row = list[i];
			// 对应 Java 的 ExcelBuilder.read(...).validate(true)：C# 端无校验管线，这里手工校验必填项
			var rowNo = i + 1;
			if (!row.DeptId.HasValue)
			{
				throw new ServiceException($"第{rowNo}行导入失败：部门id不能为空");
			}

			if (!row.UserId.HasValue)
			{
				throw new ServiceException($"第{rowNo}行导入失败：用户id不能为空");
			}

			if (!row.OrderNum.HasValue)
			{
				throw new ServiceException($"第{rowNo}行导入失败：排序号不能为空");
			}

			if (!row.TestKey.IsNotNullOrWhiteSpace())
			{
				throw new ServiceException($"第{rowNo}行导入失败：key键不能为空");
			}

			if (!row.Value.IsNotNullOrWhiteSpace())
			{
				throw new ServiceException($"第{rowNo}行导入失败：值不能为空");
			}

			entities.Add(row.MapTo<TTestDemo>(mapper));
		}

		await SaveBatchAsync(entities);
		return $"导入成功{entities.Count}条数据";
	}

	/// <summary>
	/// 构造测试单表列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">测试单表筛选条件</param>
	/// <returns>测试单表列表查询对象</returns>
	private ISelect<TTestDemo> BuildDemoQuery(TestDemoBo bo)
	{
		return fsql.Select<TTestDemo>()
			// 对应 Java 实体上的 @TableLogic：查询自动追加 del_flag = 0
			.Where(x => x.DelFlag == DEL_FLAG_NORMAL)
			.WhereNotNullEq(bo.DeptId, x => x.DeptId)
			.WhereNotNullEq(bo.UserId, x => x.UserId)
			.WhereLike(bo.TestKey, x => x.TestKey)
			.WhereHasTextEq(bo.Value, x => x.Value)
			// 对应 Java 实体上的 @OrderBy(asc = false, sort = 1)
			.OrderByDescending(x => x.OrderNum)
			// 对应 buildQueryWrapper 中的 orderByAsc(TestDemo::getId)
			.OrderBy(x => x.Id);
	}

	/// <summary>
	/// 保存前的数据校验（对应 Java 的 <c>validEntityBeforeSave</c>）
	/// </summary>
	/// <param name="entity">实体类数据</param>
	private static void ValidEntityBeforeSave(TTestDemo entity)
	{
		// TODO 做一些数据校验，如唯一约束
	}

	/// <summary>
	/// 回填创建人 / 更新人账号（对应 Java 的 @Translation USER_ID_TO_NAME）
	/// </summary>
	/// <param name="rows">测试单表列表</param>
	private async Task FillUserName(List<TestDemoVo> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		var userIds = rows.Where(x => x.CreateBy.HasValue).Select(x => x.CreateBy.Value)
			.Concat(rows.Where(x => x.UpdateBy.HasValue).Select(x => x.UpdateBy.Value))
			.Distinct()
			.ToList();
		if (userIds.Count == 0)
		{
			return;
		}

		var users = await fsql.Select<TSysUser>()
			.Where(x => userIds.Contains(x.UserId))
			.ToListAsync(x => new { x.UserId, x.UserName });
		var userNames = users.ToDictionary(x => x.UserId, x => x.UserName);

		foreach (var row in rows)
		{
			if (row.CreateBy.HasValue && userNames.TryGetValue(row.CreateBy.Value, out var createByName))
			{
				row.CreateByName = createByName;
			}

			if (row.UpdateBy.HasValue && userNames.TryGetValue(row.UpdateBy.Value, out var updateByName))
			{
				row.UpdateByName = updateByName;
			}
		}
	}
}
