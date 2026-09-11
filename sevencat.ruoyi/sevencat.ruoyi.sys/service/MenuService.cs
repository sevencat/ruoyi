using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.entity;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 菜单业务层（对应 Java 的 <c>ISysMenuService</c> / <c>SysMenuServiceImpl</c>）
/// </summary>
/// <remarks>
/// Java 端菜单信息缓存在 Redis（<c>CacheNames.SYS_MENU</c>），C# 端暂无该缓存，
/// 相关 <c>@Cacheable</c> / <c>@CacheEvict</c> 均无需实现。
/// Java 接口中 <c>selectMenuPermsByUserId</c> / <c>selectMenuPermsByRoleId</c> /
/// <c>selectMenuListByUserId</c> 等权限查询入口属于其他模块调用方，本项目暂无调用方，未实现。
/// </remarks>
[Component]
public class MenuService(IFreeSql fsql, IMapper mapper, LoginService loginService)
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

	/// <summary>
	/// 查询当前用户可访问的菜单树（对应 Java 的 <c>selectMenuTreeByUserId</c>）
	/// </summary>
	/// <param name="uid">用户ID</param>
	/// <returns>菜单树</returns>
	public async Task<List<TSysMenu>> GetMenusByUid(long uid)
	{
		List<TSysMenu> menus;
		if (LoginUser.IsSuperAdmin(uid))
		{
			menus = await fsql.Select<TSysMenu>()
				.Where(x => x.MenuType == SystemConstants.TYPE_DIR || x.MenuType == SystemConstants.TYPE_MENU)
				.Where(x => x.Status == SystemConstants.NORMAL)
				.OrderBy(x => x.ParentId)
				.OrderBy(x => x.OrderNum)
				.ToListAsync();
		}
		else
		{
			// 对应 Java 联表条件 sr.status = '0'，先取出用户拥有的正常状态角色
			var roleIds = await SelectNormalRoleIdsByUser(uid);
			if (roleIds.Count == 0)
			{
				return new List<TSysMenu>();
			}

			menus = await fsql.Select<TSysMenu, TSysRoleMenu, TSysUserRole>()
				.InnerJoin(x => x.t1.MenuId == x.t2.MenuId)
				.InnerJoin(x => x.t2.RoleId == x.t3.RoleId)
				.Where(x => x.t3.UserId == uid)
				.Where(x => roleIds.Contains(x.t2.RoleId))
				.Where(x => x.t1.MenuType == SystemConstants.TYPE_DIR || x.t1.MenuType == SystemConstants.TYPE_MENU)
				.Where(x => x.t1.Status == SystemConstants.NORMAL)
				.OrderBy(x => x.t1.ParentId)
				.OrderBy(x => x.t1.OrderNum)
				.ToListAsync(x => x.t1);

			// 对应 Java 的 distinct，多角色会关联出重复菜单
			menus = menus.DistinctBy(x => x.MenuId).ToList();
		}

		if (menus == null || menus.Count == 0)
		{
			return new List<TSysMenu>();
		}

		// 使用动态规划构建树形结构
		var menuTree = Build<long, TSysMenu>(
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
	/// 查询菜单列表（对应 Java 的 <c>selectMenuList</c>）
	/// </summary>
	/// <param name="menu">菜单查询条件</param>
	/// <param name="userId">用户ID</param>
	/// <returns>菜单列表</returns>
	public async Task<List<SysMenuVo>> SelectMenuList(SysMenuBo menu, long userId)
	{
		menu ??= new SysMenuBo();
		List<TSysMenu> rows;
		if (LoginUser.IsSuperAdmin(userId))
		{
			rows = await BuildMenuQuery(menu).ToListAsync();
		}
		else
		{
			var roleIds = await SelectNormalRoleIdsByUser(userId);
			if (roleIds.Count == 0)
			{
				return [];
			}

			var query = fsql.Select<TSysMenu, TSysRoleMenu, TSysUserRole>()
				.InnerJoin(x => x.t1.MenuId == x.t2.MenuId)
				.InnerJoin(x => x.t2.RoleId == x.t3.RoleId)
				.Where(x => x.t3.UserId == userId)
				.Where(x => roleIds.Contains(x.t2.RoleId));
			if (!string.IsNullOrWhiteSpace(menu.MenuName))
			{
				query = query.Where(x => x.t1.MenuName.Contains(menu.MenuName));
			}

			if (!string.IsNullOrWhiteSpace(menu.Visible))
			{
				query = query.Where(x => x.t1.Visible == menu.Visible);
			}

			if (!string.IsNullOrWhiteSpace(menu.Status))
			{
				query = query.Where(x => x.t1.Status == menu.Status);
			}

			if (!string.IsNullOrWhiteSpace(menu.MenuType))
			{
				query = query.Where(x => x.t1.MenuType == menu.MenuType);
			}

			if (menu.ParentId.HasValue)
			{
				query = query.Where(x => x.t1.ParentId == menu.ParentId);
			}

			rows = (await query
					.OrderBy(x => x.t1.ParentId)
					.OrderBy(x => x.t1.OrderNum)
					.ToListAsync(x => x.t1))
				.DistinctBy(x => x.MenuId)
				.ToList();
		}

		return rows.MapTo<List<SysMenuVo>>(mapper);
	}

	/// <summary>
	/// 根据菜单编号查询菜单详情（对应 Java 的 <c>selectMenuById</c>）
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>菜单详情，不存在返回 null</returns>
	public async Task<SysMenuVo> SelectMenuById(long menuId)
	{
		var menu = await fsql.Select<TSysMenu>()
			.Where(x => x.MenuId == menuId)
			.FirstAsync();
		return menu?.MapTo<SysMenuVo>(mapper);
	}

	/// <summary>
	/// 根据角色ID查询菜单树信息（对应 Java 的 <c>selectMenuListByRoleId</c>）
	/// </summary>
	/// <param name="roleId">角色ID</param>
	/// <returns>选中菜单集合</returns>
	public async Task<List<long>> SelectMenuListByRoleId(long roleId)
	{
		var role = await fsql.Select<TSysRole>()
			.Where(x => x.RoleId == roleId)
			.FirstAsync();
		if (role == null || !SystemConstants.NORMAL.Equals(role.Status))
		{
			return [];
		}

		var menus = await fsql.Select<TSysMenu, TSysRoleMenu>()
			.InnerJoin(x => x.t1.MenuId == x.t2.MenuId)
			.Where(x => x.t2.RoleId == roleId)
			.OrderBy(x => x.t1.ParentId)
			.OrderBy(x => x.t1.OrderNum)
			.ToListAsync(x => x.t1);

		var distinctMenus = menus.DistinctBy(x => x.MenuId).ToList();
		var menuIds = distinctMenus.Select(x => x.MenuId).ToList();
		if (role.MenuCheckStrictly ?? false)
		{
			return menuIds;
		}

		// 非严格模式下需要排除父节点，交由前端按父子联动回显
		var parentIds = distinctMenus
			.Where(x => x.ParentId.HasValue)
			.Select(x => x.ParentId.Value)
			.ToHashSet();
		return menuIds.Where(id => !parentIds.Contains(id)).ToList();
	}

	/// <summary>
	/// 构建前端所需要下拉树结构（对应 Java 的 <c>buildMenuTreeSelect</c>）
	/// </summary>
	/// <param name="menus">菜单列表</param>
	/// <returns>下拉树结构列表</returns>
	public List<MenuTreeSelectNode> BuildMenuTreeSelect(List<SysMenuVo> menus)
	{
		if (menus == null || menus.Count == 0)
		{
			return [];
		}

		var nodeMaps = new Dictionary<long, MenuTreeSelectNode>();
		foreach (var menu in menus)
		{
			var menuId = menu.MenuId ?? 0L;
			nodeMaps[menuId] = new MenuTreeSelectNode
			{
				Id = menuId,
				ParentId = menu.ParentId ?? 0L,
				Label = menu.MenuName,
				Weight = menu.OrderNum ?? 0,
				MenuType = menu.MenuType,
				Icon = menu.Icon,
				Visible = menu.Visible,
				Status = menu.Status,
				Children = []
			};
		}

		var tree = new List<MenuTreeSelectNode>();
		foreach (var node in nodeMaps.Values)
		{
			// 父节点不存在时视为根节点
			if (nodeMaps.TryGetValue(node.ParentId, out var parent))
			{
				parent.Children.Add(node);
			}
			else
			{
				tree.Add(node);
			}
		}

		return tree;
	}

	/// <summary>
	/// 校验菜单名称是否唯一（对应 Java 的 <c>checkMenuNameUnique</c>）
	/// </summary>
	/// <param name="menu">菜单信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckMenuNameUnique(SysMenuBo menu)
	{
		var exist = await fsql.Select<TSysMenu>()
			.Where(x => x.MenuName == menu.MenuName)
			.Where(x => x.ParentId == menu.ParentId)
			.WhereIf(menu.MenuId.HasValue, x => x.MenuId != menu.MenuId.Value)
			.AnyAsync();
		return !exist;
	}

	/// <summary>
	/// 校验路由名称或地址是否唯一（对应 Java 的 <c>checkRouteConfigUnique</c>）
	/// </summary>
	/// <param name="menuBo">菜单信息</param>
	/// <returns>true 唯一 false 不唯一</returns>
	public async Task<bool> CheckRouteConfigUnique(SysMenuBo menuBo)
	{
		if (SystemConstants.TYPE_BUTTON.Equals(menuBo.MenuType))
		{
			return true;
		}

		var menu = menuBo.MapTo<TSysMenu>(mapper);
		var menuId = menuBo.MenuId ?? -1L;
		var parentId = menuBo.ParentId;
		var path = menuBo.Path;
		var routeName = string.IsNullOrEmpty(menu.GetRouteName()) ? path : menu.GetRouteName();

		var sysMenuList = await fsql.Select<TSysMenu>()
			.Where(x => x.MenuType == SystemConstants.TYPE_DIR || x.MenuType == SystemConstants.TYPE_MENU)
			.Where(x => x.Path == path || x.Path == routeName)
			.ToListAsync();

		foreach (var sysMenu in sysMenuList)
		{
			if (sysMenu.MenuId == menuId)
			{
				continue;
			}

			var dbParentId = sysMenu.ParentId;
			var dbPath = sysMenu.Path;
			var dbRouteName = string.IsNullOrEmpty(sysMenu.GetRouteName()) ? dbPath : sysMenu.GetRouteName();

			if (string.Equals(path, dbPath, StringComparison.OrdinalIgnoreCase) && parentId == dbParentId)
			{
				return false;
			}

			if (string.Equals(path, dbPath, StringComparison.OrdinalIgnoreCase)
			    && Constants.TOP_PARENT_ID == parentId
			    && Constants.TOP_PARENT_ID == dbParentId)
			{
				return false;
			}

			if (string.Equals(routeName, dbRouteName, StringComparison.OrdinalIgnoreCase)
			    && sysMenu.MenuType == menuBo.MenuType)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// 新增保存菜单信息（对应 Java 的 <c>insertMenu</c>）
	/// </summary>
	/// <param name="bo">菜单信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> InsertMenu(SysMenuBo bo)
	{
		var menu = bo.MapTo<TSysMenu>(mapper);
		// 对应 Java 的 InjectionMetaObjectHandler 自动填充创建人
		menu.CreateBy ??= await loginService.GetLoginuid();
		return await fsql.Insert(menu).ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 修改保存菜单信息（对应 Java 的 <c>updateMenu</c>）
	/// </summary>
	/// <param name="bo">菜单信息</param>
	/// <returns>影响行数</returns>
	public async Task<int> UpdateMenu(SysMenuBo bo)
	{
		var menu = bo.MapTo<TSysMenu>(mapper);
		menu.UpdateBy ??= await loginService.GetLoginuid();
		// SetSourceIgnore 对应 MyBatis-Plus updateById 的「null 不更新」语义
		return await fsql.Update<TSysMenu>()
			.SetSourceIgnore(menu)
			.Where(x => x.MenuId == menu.MenuId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 删除菜单管理信息（对应 Java 的 <c>deleteMenuById(Long menuId)</c>）
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>影响行数</returns>
	public async Task<int> DeleteMenuById(long menuId)
	{
		return await fsql.Delete<TSysMenu>()
			.Where(x => x.MenuId == menuId)
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 批量删除菜单管理信息（对应 Java 的 <c>deleteMenuById(Long[] menuIds)</c>）
	/// </summary>
	/// <param name="menuIds">菜单ID集合</param>
	/// <returns>影响行数</returns>
	// Java 原注解 @Transactional(rollbackFor = Exception.class)：C# 端未引入事务包装，多次写库未置于同一事务
	public async Task<int> DeleteMenuById(List<long> menuIds)
	{
		if (menuIds is not { Count: > 0 })
		{
			return 0;
		}

		await fsql.Delete<TSysRoleMenu>()
			.Where(x => menuIds.Contains(x.MenuId))
			.ExecuteAffrowsAsync();
		return await fsql.Delete<TSysMenu>()
			.Where(x => menuIds.Contains(x.MenuId))
			.ExecuteAffrowsAsync();
	}

	/// <summary>
	/// 查询菜单是否存在子节点（对应 Java 的 <c>hasChildByMenuId(Long menuId)</c>）
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>true 存在 false 不存在</returns>
	public async Task<bool> HasChildByMenuId(long menuId)
	{
		return await fsql.Select<TSysMenu>()
			.Where(x => x.ParentId == menuId)
			.AnyAsync();
	}

	/// <summary>
	/// 查询菜单是否存在子节点（对应 Java 的 <c>hasChildByMenuId(Collection&lt;Long&gt; menuIds)</c>）
	/// </summary>
	/// <param name="menuIds">菜单ID集合</param>
	/// <returns>true 存在 false 不存在</returns>
	public async Task<bool> HasChildByMenuId(List<long> menuIds)
	{
		if (menuIds is not { Count: > 0 })
		{
			return false;
		}

		return await fsql.Select<TSysMenu>()
			.Where(x => x.ParentId != null && menuIds.Contains(x.ParentId.Value))
			.Where(x => !menuIds.Contains(x.MenuId))
			.AnyAsync();
	}

	/// <summary>
	/// 查询菜单是否已分配角色（对应 Java 的 <c>checkMenuExistRole</c>）
	/// </summary>
	/// <param name="menuId">菜单ID</param>
	/// <returns>true 已分配 false 未分配</returns>
	public async Task<bool> CheckMenuExistRole(long menuId)
	{
		return await fsql.Select<TSysRoleMenu>()
			.Where(x => x.MenuId == menuId)
			.AnyAsync();
	}

	/// <summary>
	/// 查询用户拥有的正常状态角色ID（对应 Java 联表条件 <c>sr.status = '0'</c>）
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <returns>角色ID集合</returns>
	private async Task<List<long>> SelectNormalRoleIdsByUser(long userId)
	{
		return await fsql.Select<TSysUserRole, TSysRole>()
			.InnerJoin(x => x.t1.RoleId == x.t2.RoleId)
			.Where(x => x.t1.UserId == userId)
			.Where(x => x.t2.Status == SystemConstants.NORMAL)
			.ToListAsync(x => x.t1.RoleId);
	}

	/// <summary>
	/// 构建菜单查询条件（对应 Java <c>selectMenuList</c> 的 LambdaQueryWrapper）
	/// </summary>
	/// <param name="menu">菜单查询条件</param>
	/// <returns>查询对象</returns>
	private ISelect<TSysMenu> BuildMenuQuery(SysMenuBo menu)
	{
		return fsql.Select<TSysMenu>()
			.WhereLike(menu.MenuName, x => x.MenuName)
			.WhereHasTextEq(menu.Visible, x => x.Visible)
			.WhereHasTextEq(menu.Status, x => x.Status)
			.WhereHasTextEq(menu.MenuType, x => x.MenuType)
			.WhereNotNullEq(menu.ParentId, x => x.ParentId)
			.OrderBy(x => x.ParentId)
			.OrderBy(x => x.OrderNum);
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
