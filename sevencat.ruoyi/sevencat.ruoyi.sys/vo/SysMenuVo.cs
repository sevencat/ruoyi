namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 菜单权限视图对象 sys_menu
/// </summary>
// Java 原注解 @AutoMapper(target = SysMenu.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
public class SysMenuVo
{
	/// <summary>
	/// 菜单ID
	/// </summary>
	public long? MenuId { get; set; }

	/// <summary>
	/// 菜单名称
	/// </summary>
	public string MenuName { get; set; }

	/// <summary>
	/// 父菜单ID
	/// </summary>
	public long? ParentId { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	public int? OrderNum { get; set; }

	/// <summary>
	/// 路由地址
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// 组件路径
	/// </summary>
	public string Component { get; set; }

	/// <summary>
	/// 路由参数
	/// </summary>
	public string QueryParam { get; set; }

	/// <summary>
	/// 是否为外链（Y是 N否）
	/// </summary>
	public string IsFrame { get; set; }

	/// <summary>
	/// 是否缓存（Y缓存 N不缓存）
	/// </summary>
	public string IsCache { get; set; }

	/// <summary>
	/// 菜单类型（M目录 C菜单 F按钮）
	/// </summary>
	public string MenuType { get; set; }

	/// <summary>
	/// 显示状态（0显示 1隐藏）
	/// </summary>
	public string Visible { get; set; }

	/// <summary>
	/// 菜单状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 权限标识
	/// </summary>
	public string Perms { get; set; }

	/// <summary>
	/// 菜单图标
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	/// 激活菜单路径
	/// </summary>
	public string ActiveMenu { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	public string Ext { get; set; }

	/// <summary>
	/// 创建部门
	/// </summary>
	public long? CreateDept { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }

	/// <summary>
	/// 子菜单
	/// </summary>
	public List<SysMenuVo> Children { get; set; } = [];
}
