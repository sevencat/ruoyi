using FreeSql;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 部门管理业务层 —— 查询条件构造、树结构维护等私有辅助实现
/// </summary>
/// <remarks>
/// 对外接口与主流程见 <c>SysDeptService.cs</c>，本文件只承载不对外暴露的辅助逻辑。
/// </remarks>
public partial class SysDeptService
{
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
	private void UpdateDeptChildren(long deptId, string newAncestors, string oldAncestors)
	{
		var children = fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToList();

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
			fsql.Update<TSysDept>()
				.Set(a => a.Ancestors, dept.Ancestors)
				.Where(a => a.DeptId == dept.DeptId)
				.ExecuteAffrows();
		}
	}

	/// <summary>
	/// 修改该部门的父级部门状态（对应 Java 的 <c>updateParentDeptStatusNormal</c>）
	/// </summary>
	/// <param name="ancestors">当前部门的祖级列表，如 0,100,101</param>
	private void UpdateParentDeptStatusNormal(string ancestors)
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

		fsql.Update<TSysDept>()
			.Set(a => a.Status, SystemConstants.NORMAL)
			.Where(a => deptIds.Contains(a.DeptId))
			.ExecuteAffrows();
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
}
