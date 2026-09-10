namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 路由显示信息
/// </summary>
public class MetaVo
{
	/// <summary>
	/// 设置该路由在侧边栏和面包屑中展示的名字
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// 设置该路由的图标，对应路径src/assets/icons/svg
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	/// 设置为true，则不会被 &lt;keep-alive&gt; 缓存
	/// </summary>
	public bool? NoCache { get; set; }

	/// <summary>
	/// 内链地址（http(s)://开头）
	/// </summary>
	public string Link { get; set; }

	/// <summary>
	/// 激活菜单
	/// </summary>
	public string ActiveMenu { get; set; }

	/// <summary>
	/// 构造路由显示信息。
	/// </summary>
	/// <param name="title">路由标题</param>
	/// <param name="icon">路由图标</param>
	public MetaVo(string title, string icon)
	{
		Title = title;
		Icon = icon;
	}

	/// <summary>
	/// 构造路由显示信息。
	/// </summary>
	/// <param name="title">路由标题</param>
	/// <param name="icon">路由图标</param>
	/// <param name="noCache">是否不缓存</param>
	public MetaVo(string title, string icon, bool? noCache)
	{
		Title = title;
		Icon = icon;
		NoCache = noCache;
	}

	/// <summary>
	/// 构造带内链地址的路由显示信息。
	/// </summary>
	/// <param name="title">路由标题</param>
	/// <param name="icon">路由图标</param>
	/// <param name="link">内链地址</param>
	public MetaVo(string title, string icon, string link)
	{
		Title = title;
		Icon = icon;
		Link = link;
	}

	/// <summary>
	/// 构造带缓存配置和内链地址的路由显示信息。
	/// </summary>
	/// <param name="title">路由标题</param>
	/// <param name="icon">路由图标</param>
	/// <param name="noCache">是否不缓存</param>
	/// <param name="link">内链地址</param>
	public MetaVo(string title, string icon, bool? noCache, string link)
	{
		Title = title;
		Icon = icon;
		NoCache = noCache;
		if (IsHttp(link))
		{
			Link = link;
		}
	}

	/// <summary>
	/// 构造带激活菜单的路由显示信息。
	/// </summary>
	/// <param name="title">路由标题</param>
	/// <param name="icon">路由图标</param>
	/// <param name="noCache">是否不缓存</param>
	/// <param name="link">内链地址</param>
	/// <param name="activeMenu">激活菜单路径</param>
	public MetaVo(string title, string icon, bool? noCache, string link, string activeMenu)
	{
		Title = title;
		Icon = icon;
		NoCache = noCache;
		if (IsHttp(link))
		{
			Link = link;
		}
		if (StartWithAnyIgnoreCase(activeMenu, "/"))
		{
			ActiveMenu = activeMenu;
		}
	}

	/// <summary>
	/// 判断是否为 http(s) 链接（对应 Java StringUtils.ishttp）
	/// </summary>
	private static bool IsHttp(string link)
	{
		if (string.IsNullOrEmpty(link))
		{
			return false;
		}
		return link.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
			|| link.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// 判断字符串是否以任意指定前缀开头（忽略大小写，对应 Java StringUtils.startWithAnyIgnoreCase）
	/// </summary>
	private static bool StartWithAnyIgnoreCase(string str, params string[] prefixes)
	{
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}
		foreach (var prefix in prefixes)
		{
			if (str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
