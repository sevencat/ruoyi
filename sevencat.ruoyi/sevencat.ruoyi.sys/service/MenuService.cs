using Autofac.Annotation;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.constant;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

[Component]
public class MenuService(IFreeSql fsql)
{
	/// <summary>
	/// 构建前端路由
	/// </summary>
	/// <param name="menus">菜单列表</param>
	public List<RouterVo> BuildMenus(List<TSysMenu> menus)
	{
		if (menus == null || menus.Count == 0)
		{
			return new List<RouterVo>();
		}

		var routers = new List<RouterVo>();
		foreach (var menu in menus)
		{
			var name = menu.GetRouteName() + menu.MenuId;

			var router = new RouterVo();
			router.Hidden = "1".Equals(menu.Visible);
			router.Name = name;
			router.Path = menu.GetRouterPath();
			router.Component = menu.GetComponentInfo();
			router.Query = menu.QueryParam;
			router.Ext = menu.Ext;
			router.Meta = new MetaVo(menu.MenuName, menu.Icon, SystemConstants.NO.Equals(menu.IsCache), menu.Path,
				menu.ActiveMenu);

			var cMenus = menu.Children;
			if (cMenus != null && cMenus.Count > 0 && SystemConstants.TYPE_DIR.Equals(menu.MenuType))
			{
				router.AlwaysShow = true;
				router.Redirect = "noRedirect";
				router.Children = BuildMenus(cMenus);
			}
			else if (menu.IsMenuFrame())
			{
				var frameName = CommonStrUtil.Capitalize(menu.Path) + menu.MenuId;
				router.Meta = null;
				var childrenList = new List<RouterVo>();
				var children = new RouterVo();
				children.Path = menu.Path;
				children.Component = menu.Component;
				children.Name = frameName;
				children.Meta = new MetaVo(menu.MenuName, menu.Icon, SystemConstants.NO.Equals(menu.IsCache), menu.Path,
					menu.ActiveMenu);
				children.Query = menu.QueryParam;
				children.Ext = menu.Ext;
				childrenList.Add(children);
				router.Children = childrenList;
			}
			else if (menu.ParentId == Constants.TOP_PARENT_ID && menu.IsInnerLink())
			{
				router.Meta = new MetaVo(menu.MenuName, menu.Icon);
				router.Path = "/";
				var childrenList = new List<RouterVo>();
				var children = new RouterVo();
				var routerPath = TSysMenu.InnerLinkReplaceEach(menu.Path);
				var innerLinkName = CommonStrUtil.Capitalize(routerPath) + menu.MenuId;
				children.Path = routerPath;
				children.Component = SystemConstants.INNER_LINK;
				children.Name = innerLinkName;
				children.Meta = new MetaVo(menu.MenuName, menu.Icon, menu.Path);
				children.Ext = menu.Ext;
				childrenList.Add(children);
				router.Children = childrenList;
			}

			routers.Add(router);
		}

		return routers;
	}

	public async Task<List<TSysMenu>> GetMenusByUid(long uid)
	{
		List<TSysMenu> menus = null;
		if (LoginUser.IsSuperAdmin(uid))
		{
			menus = await fsql.Select<TSysMenu>().ToListAsync();
		}
		else
		{
			menus = await fsql.Select<TSysMenu, TSysRoleMenu, TSysUserRole>()
				.InnerJoin(x => x.t1.MenuId == x.t2.MenuId)
				.InnerJoin(x => x.t2.RoleId == x.t3.RoleId)
				.Where(x => x.t3.UserId == uid)
				.ToListAsync(x => x.t1);
		}

		if (menus == null || menus.Count == 0)
		{
			return new List<TSysMenu>();
		}

		// 使用动态规划构建树形结构
		var menuTree = MenuService.Build<long, TSysMenu>(
			menus,
			Constants.TOP_PARENT_ID,
			menu => menu.ParentId ?? 0L,
			(menu, nodeTreeMaps) =>
			{
				// 将当前节点的菜单ID用作父节点ID，从动态规划表中取出子节点列表
				// 如果不存在子节点，则返回一个空的列表，确保数据在进行JSON序列化时该字段的类型和结构是正确的
				var childMenus = nodeTreeMaps.GetValueOrDefault(menu.MenuId, new List<TSysMenu>());
				menu.Children = childMenus;
			});

		return menuTree ?? new List<TSysMenu>();
	}


	/// <summary>
	/// 使用动态规划构建树形结构
	/// </summary>
	/// <param name="items">节点列表项</param>
	/// <param name="parentId">父节点ID</param>
	/// <param name="classifier">动态规划表分类函数</param>
	/// <param name="action">回溯动作</param>
	/// <typeparam name="K">节点ID的类型</typeparam>
	/// <typeparam name="T">输入节点的类型</typeparam>
	/// <returns>构建好的树形结构列表</returns>
	public static List<T> Build<K, T>(List<T> items, K parentId, Func<T, K> classifier,
		Action<T, Dictionary<K, List<T>>> action)
	{
		// 构建动态规划表 (依据父ID分组)
		var nodeTreeMaps = items.GroupBy(classifier).ToDictionary(g => g.Key, g => g.ToList());
		// 回溯构建各级节点关系
		foreach (var item in items)
		{
			action(item, nodeTreeMaps);
		}

		// 对应 Java 的 nodeTreeMaps.get(parentId)：不存在时返回 null
		nodeTreeMaps.TryGetValue(parentId, out var tree);
		return tree;
	}
}