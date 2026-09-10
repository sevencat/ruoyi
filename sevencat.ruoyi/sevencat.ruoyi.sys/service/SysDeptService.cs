using Autofac.Annotation;
using FreeSql;
using sevencat.common;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysDeptService(IFreeSql fsql)
{
	/// <summary>
	/// 查询部门树结构信息
	/// </summary>
	/// <param name="bo">部门信息</param>
	/// <returns>部门树信息集合</returns>
	public async Task<List<TreeSelectNode<TSysDept>>> SelectDeptTreeList(SysDeptBo bo)
	{
		var depts = await BuildDeptQuery(bo).ToListAsync();

		// 部门树搜索：仅保留指定部门及其所有子部门
		if (bo.BelongDeptId.HasValue)
		{
			var belongId = bo.BelongDeptId.Value;
			var suffix = $"{belongId},";
			depts = depts
				.Where(dept => dept.DeptId == belongId || $"{dept.Ancestors},".Contains(suffix))
				.ToList();
		}

		return BuildDeptTreeSelect(depts);
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
			.OrderBy(x => x.Ancestors)
			.OrderBy(x => x.ParentId)
			.OrderBy(x => x.OrderNum)
			.OrderBy(x => x.DeptId);
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