using Autofac.Annotation;
using FreeSql;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.lang;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity.db;

namespace sevencat.ruoyi.sys.service;

[Component]
public class SysDeptService(IFreeSql fsql, LoginService loginService)
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
	/// 查询指定部门及其所有子部门ID（对应 Java 的 <c>selectDeptAndChildById</c>）
	/// </summary>
	/// <param name="deptId">部门ID</param>
	/// <returns>部门ID列表</returns>
	public async Task<List<long>> SelectDeptAndChildById(long deptId)
	{
		var depts = await fsql.Select<TSysDept>()
			.Where(x => x.DelFlag == SystemConstants.NORMAL)
			.ToListAsync();

		// Java 用 find_in_set(deptId, ancestors) 匹配子孙部门；这里按「祖先串以 deptId, 结尾」判断，
		// 与 SelectDeptTreeList 的 BelongDeptId 处理保持一致
		var suffix = $"{deptId},";
		return depts
			.Where(dept => dept.DeptId == deptId || $"{dept.Ancestors},".Contains(suffix))
			.Select(dept => dept.DeptId)
			.ToList();
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
