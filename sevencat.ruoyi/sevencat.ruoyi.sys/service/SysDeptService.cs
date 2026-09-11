using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 部门管理业务层（对应 Java 的 <c>ISysDeptService</c> / <c>SysDeptServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端部门信息缓存在 Redis（<c>CacheNames.SYS_DEPT</c> / <c>CacheNames.SYS_DEPT_AND_CHILD</c>），
/// C# 端暂无该缓存，相关 <c>@Cacheable</c> / <c>@CacheEvict</c> / <c>CacheUtils.evict</c> 均无需实现。
/// Java 接口中 <c>selectPageDeptList</c> / <c>selectDeptListByRoleId</c> / <c>selectDeptNameByIds</c> /
/// <c>selectDeptLeaderById</c> / <c>selectDeptsByList</c> / <c>selectDeptNamesByIds</c> 属于其他模块的调用入口，
/// 本项目暂无调用方，未实现。
/// </remarks>
[Component]
public class SysDeptService(IFreeSql fsql, IMapper mapper, LoginService loginService)
{
	/// <summary>
	/// 删除标志（0代表存在 1代表删除），对应 Java 实体字段上的 <c>@TableLogic</c> 逻辑删除
	/// </summary>
	private const string DEL_FLAG_DELETED = "1";

	/// <summary>
	/// 查询部门树结构信息（对应 Java 的 <c>selectDeptTreeList</c>）
	/// </summary>
	/// <param name="bo">部门信息</param>
	/// <returns>部门树信息集合</returns>
	public async Task<List<TreeSelectNode<TSysDept>>> SelectDeptTreeList(SysDeptBo bo)
	{
		return BuildDeptTreeSelect(await QueryDeptRows(bo));
	}

	/// <summary>
	/// 查询部门管理数据（对应 Java 的 <c>selectDeptList</c>）
	/// </summary>
	/// <param name="bo">部门信息</param>
	/// <returns>部门信息集合</returns>
	public async Task<List<SysDeptVo>> SelectDeptList(SysDeptBo bo)
	{
		var depts = await QueryDeptRows(bo);
		return depts.MapTo<List<SysDeptVo>>(mapper);
	}

	/// <summary>
	/// 查询指定部门及其所有子部门ID（对应 Java 的 <c>selectDeptAndChildById</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门ID列表</returns>
	public async Task<List<long>> SelectDeptAndChildById(long deptId)
	{
		var depts = await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToListAsync();

		// Java 用 find_in_set(deptId, ancestors) 匹配子孙部门；这里按「祖先串的元素包含 deptId」判断
		return depts
			.Where(dept => IsDeptOrChild(dept, deptId))
			.Select(dept => dept.DeptId)
			.ToList();
	}

	/// <summary>
	/// 根据部门ID查询信息（对应 Java 的 <c>selectDeptById</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门信息（含上级部门名称）；不存在时返回 null</returns>
	// Java 原注解 @Cacheable(cacheNames = CacheNames.SYS_DEPT, key = "#deptId")：C# 端无部门缓存，未实现
	public async Task<SysDeptVo> SelectDeptById(long deptId)
	{
		var dept = await fsql.Select<TSysDept>()
			.Where(x => x.DeptId == deptId)
			.FirstAsync();
		if (dept == null)
		{
			return null;
		}

		var vo = dept.MapTo<SysDeptVo>(mapper);

		// 对应 Java 的 ObjectUtils.notNullGetter(parentDept, SysDeptVo::getDeptName)：父部门不存在时保持 null
		vo.ParentName = await fsql.Select<TSysDept>()
			.Where(x => x.DeptId == dept.ParentId)
			.FirstAsync(x => x.DeptName);

		return vo;
	}

	/// <summary>
	/// 按部门主键集合查询部门基础信息（对应 Java 的 <c>selectDeptByIds</c>）
	/// </summary>
	/// <param name="deptIds">部门主键集合；为空时不追加主键过滤（返回全部正常状态部门）</param>
	/// <returns>部门基础信息列表</returns>
	public async Task<List<SysDeptVo>> SelectDeptByIds(List<long> deptIds)
	{
		var q = fsql.Select<TSysDept>()
			.Where(x => x.Status == SystemConstants.NORMAL);

		if (deptIds is { Count: > 0 })
		{
			q = q.Where(x => deptIds.Contains(x.DeptId));
		}

		var rows = await q.ToListAsync();
		return rows.MapTo<List<SysDeptVo>>(mapper);
	}

	/// <summary>
	/// 校验部门数据权限（对应 Java 的 <c>checkDeptDataScope</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	public async Task CheckDeptDataScope(long? deptId)
	{
		if (!deptId.HasValue)
		{
			return;
		}

		var loginUser = await loginService.GetLoginUser();
		if (loginUser != null && loginUser.IsSuperAdmin())
		{
			return;
		}

		// Java 通过 DataPermissionHelper 在 countDeptById 上套部门数据权限；C# 端暂无该设施，
		// 退化为判断部门是否存在（无数据权限约束时等价于可见全部部门）
		if (!await fsql.Select<TSysDept>().Where(x => x.DeptId == deptId.Value).AnyAsync())
		{
			throw new ServiceException("没有权限访问部门数据！");
		}
	}

	/// <summary>
	/// 校验部门名称是否唯一（对应 Java 的 <c>checkDeptNameUnique</c>）
	/// </summary>
	/// <param name="dept">部门信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckDeptNameUnique(SysDeptBo dept)
	{
		var deptId = dept.DeptId;
		var exist = await fsql.Select<TSysDept>()
			.Where(x => x.DeptName == dept.DeptName)
			.Where(x => x.ParentId == dept.ParentId)
			.WhereIf(deptId.HasValue, x => x.DeptId != deptId.Value)
			.AnyAsync();

		return !exist;
	}

	/// <summary>
	/// 根据ID查询所有子部门数（正常状态）（对应 Java 的 <c>selectNormalChildrenDeptById</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>子部门数</returns>
	public async Task<long> SelectNormalChildrenDeptById(long? deptId)
	{
		if (!deptId.HasValue)
		{
			return 0;
		}

		var ancestorsList = await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.Status == SystemConstants.NORMAL)
			.ToListAsync(x => x.Ancestors);

		// 对应 Java 的 find_in_set(deptId, ancestors)：只统计子孙部门，不含自身
		return ancestorsList.Count(ancestors => IsInAncestors(ancestors, deptId.Value));
	}

	/// <summary>
	/// 是否存在子节点（对应 Java 的 <c>hasChildByDeptId</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>true 存在 false 不存在</returns>
	public async Task<bool> HasChildByDeptId(long? deptId)
	{
		if (!deptId.HasValue)
		{
			return false;
		}

		return await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.ParentId == deptId.Value)
			.AnyAsync();
	}

	/// <summary>
	/// 查询部门是否存在用户（对应 Java 的 <c>checkDeptExistUser</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>true 存在 false 不存在</returns>
	public async Task<bool> CheckDeptExistUser(long? deptId)
	{
		if (!deptId.HasValue)
		{
			return false;
		}

		return await fsql.Select<TSysUser>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.Where(x => x.DeptId == deptId.Value)
			.AnyAsync();
	}

	/// <summary>
	/// 新增保存部门信息（对应 Java 的 <c>insertDept</c>）
	/// </summary>
	/// <param name="bo">部门信息</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @CacheEvict(cacheNames = CacheNames.SYS_DEPT_AND_CHILD, allEntries = true)：C# 端无部门缓存，未实现
	public async Task<int> InsertDept(SysDeptBo bo)
	{
		var parent = bo.ParentId.HasValue
			? await fsql.Select<TSysDept>().Where(x => x.DeptId == bo.ParentId.Value).FirstAsync()
			: null;

		// 父节点不存在或不为正常状态时不允许新增子节点
		if (parent == null)
		{
			throw new ServiceException("父部门不存在");
		}

		if (!SystemConstants.NORMAL.Equals(parent.Status))
		{
			throw new ServiceException("部门停用，不允许新增");
		}

		var dept = bo.MapTo<TSysDept>(mapper);
		dept.Ancestors = $"{parent.Ancestors}{CommonStrUtil.SEPARATOR}{dept.ParentId}";
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		dept.CreateBy ??= await loginService.GetLoginuid();

		return await fsql.Insert(dept).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 修改保存部门信息（对应 Java 的 <c>updateDept</c>）
	/// </summary>
	/// <param name="bo">部门信息</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @Caching(evict = { @CacheEvict(SYS_DEPT, ...), @CacheEvict(SYS_DEPT_AND_CHILD, ...) })：C# 端无部门缓存，未实现
	// Java 原注解 @Transactional(rollbackFor = Exception.class)：C# 端未引入事务包装，多次写库未置于同一事务
	public async Task<int> UpdateDept(SysDeptBo bo)
	{
		var dept = bo.MapTo<TSysDept>(mapper);
		var oldDept = await fsql.Select<TSysDept>()
			.Where(x => x.DeptId == dept.DeptId)
			.FirstAsync();
		if (oldDept == null)
		{
			throw new ServiceException("部门不存在，无法修改");
		}

		if (oldDept.ParentId != dept.ParentId)
		{
			// 如果是新父部门，则校验是否具有新父部门权限，避免越权
			await CheckDeptDataScope(dept.ParentId);
			var newParentDept = await fsql.Select<TSysDept>()
				.Where(x => x.DeptId == dept.ParentId)
				.FirstAsync();
			if (newParentDept != null)
			{
				var newAncestors = $"{newParentDept.Ancestors}{CommonStrUtil.SEPARATOR}{newParentDept.DeptId}";
				var oldAncestors = oldDept.Ancestors;
				dept.Ancestors = newAncestors;
				await UpdateDeptChildren(dept.DeptId, newAncestors, oldAncestors);
			}
		}
		else
		{
			dept.Ancestors = oldDept.Ancestors;
		}

		dept.UpdateBy ??= await loginService.GetLoginuid();

		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		var result = await fsql.Update<TSysDept>()
			.SetSourceIgnore(dept)
			.Where(a => a.DeptId == dept.DeptId)
			.ExecuteAffrowsAsync();

		// 部门状态为启用，且祖级列表不为空、不等于根部门祖级列表（说明存在上级部门）时，
		// 一并启用该部门的所有上级部门
		if (SystemConstants.NORMAL.Equals(dept.Status)
		    && !string.IsNullOrEmpty(dept.Ancestors)
		    && !SystemConstants.ROOT_DEPT_ANCESTORS.Equals(dept.Ancestors))
		{
			await UpdateParentDeptStatusNormal(dept.Ancestors);
		}

		return result;
	}

	/// <summary>
	/// 删除部门管理信息（对应 Java 的 <c>deleteDeptById</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @Caching(evict = { @CacheEvict(SYS_DEPT, ...), @CacheEvict(SYS_DEPT_AND_CHILD, ...) })：C# 端无部门缓存，未实现
	public async Task<int> DeleteDeptById(long deptId)
	{
		// 对应 Java 的 @TableLogic 逻辑删除：deleteById 实际是 update del_flag = '1'
		return await fsql.Update<TSysDept>()
			.Set(a => a.DelFlag, DEL_FLAG_DELETED)
			.Where(a => a.DeptId == deptId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 查询部门「全路径名称」映射（对应 Java 的 <c>DeptExcelConverter</c> 构建的部门名称缓存）
	/// </summary>
	/// <returns>部门ID到「父级/子级」全路径名称的映射</returns>
	public async Task<Dictionary<long, string>> SelectDeptPathNames()
	{
		var depts = await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToListAsync();

		var byId = depts.ToDictionary(x => x.DeptId);
		var idToName = new Dictionary<long, string>(depts.Count);
		foreach (var dept in depts)
		{
			var names = new List<string>();

			// ancestors 形如 0,100,101，跳过根节点 0 后依次拼接各级父部门名称
			foreach (var ancestor in (dept.Ancestors ?? string.Empty).Split(','))
			{
				if (ancestor.IsNullOrWhiteSpace() || ancestor == "0")
				{
					continue;
				}

				if (long.TryParse(ancestor, out var ancestorId) && byId.TryGetValue(ancestorId, out var parent))
				{
					names.Add(parent.DeptName);
				}
			}

			names.Add(dept.DeptName);
			idToName[dept.DeptId] = string.Join("/", names);
		}

		return idToName;
	}

	/// <summary>
	/// 构造部门列表查询条件
	/// </summary>
	/// <param name="bo">部门筛选条件</param>
	/// <returns>部门列表查询对象</returns>
	private ISelect<TSysDept> BuildDeptQuery(SysDeptBo bo)
	{
		return fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.WhereIf(bo.DeptId.HasValue, x => x.DeptId == bo.DeptId)
			.WhereIf(bo.ParentId.HasValue, x => x.ParentId == bo.ParentId)
			.WhereIf(bo.DeptName.IsNotNullOrWhiteSpace(), x => x.DeptName.Contains(bo.DeptName))
			.WhereIf(bo.DeptCategory.IsNotNullOrWhiteSpace(), x => x.DeptCategory.Contains(bo.DeptCategory))
			.WhereIf(bo.Status.IsNotNullOrWhiteSpace(), x => x.Status == bo.Status)
			// 创建时间区间检索（对应 Java 的 params.beginTime / params.endTime）
			.WhereTimeRange(bo.Params, x => x.CreateTime)
			.OrderBy(x => x.Ancestors)
			.OrderBy(x => x.ParentId)
			.OrderBy(x => x.OrderNum)
			.OrderBy(x => x.DeptId);
	}

	/// <summary>
	/// 按查询条件取出部门行，含部门树搜索过滤（对应 Java 的 <c>buildQueryWrapper</c> + <c>selectDeptList</c>）
	/// </summary>
	/// <param name="bo">部门筛选条件</param>
	/// <returns>部门行集合</returns>
	private async Task<List<TSysDept>> QueryDeptRows(SysDeptBo bo)
	{
		var depts = await BuildDeptQuery(bo).ToListAsync();

		// 部门树搜索：仅保留指定部门及其所有子部门
		if (bo.BelongDeptId.HasValue)
		{
			var belongId = bo.BelongDeptId.Value;
			depts = depts
				.Where(dept => IsDeptOrChild(dept, belongId))
				.ToList();
		}

		return depts;
	}

	/// <summary>
	/// 修改子元素关系（对应 Java 的 <c>updateDeptChildren</c>）
	/// </summary>
	/// <param name="deptId">被修改的部门ID</param>
	/// <param name="newAncestors">新的父ID集合</param>
	/// <param name="oldAncestors">旧的父ID集合</param>
	private async Task UpdateDeptChildren(long deptId, string newAncestors, string oldAncestors)
	{
		var children = await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToListAsync();

		// 只更新祖级串，避免把名称为空等字段一并写回（对应 Java 的 updateBatchById 仅设置 deptId + ancestors）
		var list = children
			.Where(child => IsInAncestors(child.Ancestors, deptId))
			.Select(child => new
			{
				child.DeptId,
				Ancestors = CommonStrUtil.ReplaceOnce(child.Ancestors, oldAncestors, newAncestors)
			})
			.ToList();

		// Java 批量更新后会逐条 evict SYS_DEPT 缓存，C# 端无部门缓存，只做更新
		foreach (var dept in list)
		{
			await fsql.Update<TSysDept>()
				.Set(a => a.Ancestors, dept.Ancestors)
				.Where(a => a.DeptId == dept.DeptId)
				.ExecuteAffrowsAsync();
		}
	}

	/// <summary>
	/// 修改该部门的父级部门状态（对应 Java 的 <c>updateParentDeptStatusNormal</c>）
	/// </summary>
	/// <param name="ancestors">当前部门的祖级列表，如 0,100,101</param>
	private async Task UpdateParentDeptStatusNormal(string ancestors)
	{
		// Java 用 Convert.toLongArray(ancestors) 把祖级串转成 ID 数组
		var deptIds = ancestors
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(item => long.TryParse(item, out var id) ? id : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id.Value)
			.ToList();

		if (deptIds.Count == 0)
		{
			return;
		}

		await fsql.Update<TSysDept>()
			.Set(a => a.Status, SystemConstants.NORMAL)
			.Where(a => deptIds.Contains(a.DeptId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 判断部门是否属于指定部门本身或其子孙（对应 Java 的 <c>deptId.equals(...) || find_in_set(deptId, ancestors)</c>）
	/// </summary>
	/// <param name="dept">部门行</param>
	/// <param name="deptId">目标部门ID</param>
	/// <returns>true 是 false 否</returns>
	private static bool IsDeptOrChild(TSysDept dept, long deptId)
	{
		return dept.DeptId == deptId || IsInAncestors(dept.Ancestors, deptId);
	}

	/// <summary>
	/// 判断部门ID是否出现在祖级串中（对应 Java 的 <c>find_in_set(deptId, ancestors)</c>）
	/// </summary>
	/// <param name="ancestors">祖级列表，如 0,100,101</param>
	/// <param name="deptId">目标部门ID</param>
	/// <returns>true 在祖级串中 false 不在</returns>
	private static bool IsInAncestors(string ancestors, long deptId)
	{
		return $"{ancestors},".Contains($"{deptId},");
	}

	/// <summary>
	/// 构建前端所需要下拉树结构
	/// </summary>
	/// <param name="depts">部门列表</param>
	/// <returns>下拉树结构列表</returns>
	public List<TreeSelectNode<TSysDept>> BuildDeptTreeSelect(List<TSysDept> depts)
	{
		if (depts == null || depts.Count == 0)
		{
			return [];
		}

		return TreeUtil.BuildMultiRoot(
			depts,
			dept => dept.DeptId,
			dept => dept.ParentId ?? 0L,
			(dept, node) =>
			{
				node.Label = dept.DeptName;
				node.Weight = dept.OrderNum ?? 0;
				node.Disabled = SystemConstants.DISABLE.Equals(dept.Status);
				node.Data = dept;
			});
	}
}
