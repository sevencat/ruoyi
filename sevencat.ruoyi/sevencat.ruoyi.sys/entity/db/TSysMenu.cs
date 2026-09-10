using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.db.attr;
using sevencat.ruoyi.common.entity.db;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.constant;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 菜单权限表
/// </summary>
[Table(Name = "sys_menu")]
[Index("idx_sys_menu_parent_id", "ParentId")]
public class TSysMenu : TBaseEntity
{
	/// <summary>
	/// 菜单ID
	/// </summary>
	[Column(Name = "menu_id", IsPrimary = true)]
	[Snowflake]
	public long MenuId { get; set; }

	/// <summary>
	/// 菜单名称
	/// </summary>
	[Column(Name = "menu_name", StringLength = 50, IsNullable = false)]
	public string MenuName { get; set; }

	/// <summary>
	/// 父菜单ID
	/// </summary>
	[Column(Name = "parent_id", IsNullable = true)]
	public long? ParentId { get; set; } = 0;

	/// <summary>
	/// 显示顺序
	/// </summary>
	[Column(Name = "order_num", IsNullable = true)]
	public int? OrderNum { get; set; } = 0;

	/// <summary>
	/// 路由地址
	/// </summary>
	[Column(Name = "path", StringLength = 200, IsNullable = true)]
	public string Path { get; set; } = string.Empty;

	/// <summary>
	/// 组件路径
	/// </summary>
	[Column(Name = "component", StringLength = 255, IsNullable = true)]
	public string Component { get; set; }

	/// <summary>
	/// 路由参数
	/// </summary>
	[Column(Name = "query_param", StringLength = 255, IsNullable = true)]
	public string QueryParam { get; set; }

	/// <summary>
	/// 是否为外链（Y是 N否）
	/// </summary>
	[Column(Name = "is_frame", StringLength = 1, IsNullable = true)]
	public string IsFrame { get; set; } = "N";

	/// <summary>
	/// 是否缓存（Y缓存 N不缓存）
	/// </summary>
	[Column(Name = "is_cache", StringLength = 1, IsNullable = true)]
	public string IsCache { get; set; } = "Y";

	/// <summary>
	/// 菜单类型（M目录 C菜单 F按钮）
	/// </summary>
	[Column(Name = "menu_type", StringLength = 1, IsNullable = true)]
	public string MenuType { get; set; } = string.Empty;

	/// <summary>
	/// 显示状态（0显示 1隐藏）
	/// </summary>
	[Column(Name = "visible", StringLength = 1, IsNullable = true)]
	public string Visible { get; set; } = "0";

	/// <summary>
	/// 菜单状态（0正常 1停用）
	/// </summary>
	[Column(Name = "status", StringLength = 1, IsNullable = true)]
	public string Status { get; set; } = "0";

	/// <summary>
	/// 权限标识
	/// </summary>
	[Column(Name = "perms", StringLength = 100, IsNullable = true)]
	public string Perms { get; set; }

	/// <summary>
	/// 菜单图标
	/// </summary>
	[Column(Name = "icon", StringLength = 100, IsNullable = true)]
	public string Icon { get; set; } = "#";

	/// <summary>
	/// 激活菜单路径
	/// </summary>
	[Column(Name = "active_menu", StringLength = 255, IsNullable = true)]
	public string ActiveMenu { get; set; } = string.Empty;

	/// <summary>
	/// 扩展字段
	/// </summary>
	[Column(Name = "ext", StringLength = 2000, IsNullable = true)]
	public string Ext { get; set; } = string.Empty;

	[Column(IsIgnore = true)]
	public string ParentName { get; set; }

	[Column(IsIgnore = true)]
	public List<TSysMenu> Children { get; set; } = [];

	public string GetRouteName()
	{
		string routerName = CommonStrUtil.Capitalize(Path);
		// 非外链并且是一级目录（类型为目录）
		if (IsMenuFrame())
		{
			routerName = string.Empty;
		}

		return routerName;
	}

	public bool IsMenuFrame()
	{
		return (Constants.TOP_PARENT_ID == ParentId)
		       && (SystemConstants.TYPE_MENU == MenuType)
		       && (IsFrame == SystemConstants.NO);
	}

	/**
    * 是否为内链组件
    */
	public bool IsInnerLink()
	{
		return (IsFrame == SystemConstants.NO) && Path.Ishttp();
	}

	/**
     * 内链域名特殊字符替换
     */
	public static string InnerLinkReplaceEach(String path)
	{
		return CommonStrUtil.ReplaceEach(path,
			[Constants.HTTP, Constants.HTTPS, Constants.WWW, ".", CommonStrUtil.COLON],
			["", "", "", "/", "/"]);
	}

	public string GetRouterPath()
	{
		var routerPath = this.Path;
		// 内链打开外网方式
		if ((Constants.TOP_PARENT_ID != ParentId) && IsInnerLink())
		{
			routerPath = InnerLinkReplaceEach(routerPath);
		}

		// 非外链并且是一级目录（类型为目录）
		if ((Constants.TOP_PARENT_ID == ParentId)
		    && SystemConstants.TYPE_DIR == MenuType
		    && SystemConstants.NO == IsFrame)
		{
			routerPath = "/" + this.Path;
		}
		// 非外链并且是一级目录（类型为菜单）
		else if (IsMenuFrame())
		{
			routerPath = "/";
		}

		return routerPath;
	}
	
	/// <summary>
	/// 获取组件信息
	/// </summary>
	public string GetComponentInfo()
	{
		var component = SystemConstants.LAYOUT;
		if (!string.IsNullOrEmpty(this.Component) && !IsMenuFrame())
		{
			component = this.Component;
		}
		else if (string.IsNullOrEmpty(this.Component) && Constants.TOP_PARENT_ID != ParentId && IsInnerLink())
		{
			component = SystemConstants.INNER_LINK;
		}
		else if (string.IsNullOrEmpty(this.Component) && IsParentView())
		{
			component = SystemConstants.PARENT_VIEW;
		}

		return component;
	}

	/// <summary>
	/// 是否为 parent_view 组件
	/// </summary>
	public bool IsParentView()
	{
		return Constants.TOP_PARENT_ID != ParentId && SystemConstants.TYPE_DIR == MenuType;
	}
}