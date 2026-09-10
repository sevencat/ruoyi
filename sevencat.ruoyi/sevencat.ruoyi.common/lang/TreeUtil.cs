namespace sevencat.ruoyi.common.lang;

/// <summary>
/// 树结构构建工具
/// </summary>
public static class TreeUtil
{
	/// <summary>
	/// 构建多根节点的树结构（支持多个顶级节点）
	/// </summary>
	/// <param name="list">原始数据列表</param>
	/// <param name="getId">获取节点ID的方法</param>
	/// <param name="getParentId">获取节点父级ID的方法</param>
	/// <param name="parser">树节点属性映射器，用于将原始节点 T 转为树节点</param>
	/// <typeparam name="T">原始数据类型（如实体类、DTO 等）</typeparam>
	/// <returns>构建完成的树形结构（可能包含多个顶级根节点）</returns>
	public static List<TreeSelectNode<T>> BuildMultiRoot<T>(List<T> list, Func<T, long> getId,
		Func<T, long> getParentId, Action<T, TreeSelectNode<T>> parser)
	{
		if (list == null || list.Count == 0)
		{
			return [];
		}

		// 建立「节点ID -> 树节点」索引
		var nodeMap = new Dictionary<long, TreeSelectNode<T>>(list.Count);
		foreach (var item in list)
		{
			var node = new TreeSelectNode<T>
			{
				Id = getId(item),
				ParentId = getParentId(item),
				Children = []
			};
			parser(item, node);
			nodeMap[node.Id] = node;
		}

		// 父节点存在于结果集内则挂到父节点下，否则该节点即为顶级节点
		var roots = new List<TreeSelectNode<T>>();
		foreach (var node in nodeMap.Values)
		{
			if (nodeMap.TryGetValue(node.ParentId, out var parent))
			{
				parent.Children.Add(node);
			}
			else
			{
				roots.Add(node);
			}
		}

		return roots;
	}
}
