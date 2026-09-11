using System.Text.Json.Serialization;

namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 菜单下拉树节点（对应 Java 的 <c>cn.hutool.core.lang.tree.Tree&lt;Long&gt;</c>）
/// </summary>
public class MenuTreeSelectNode
{
	/// <summary>
	/// 节点ID
	/// </summary>
	[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
	public long Id { get; set; }

	/// <summary>
	/// 父节点ID
	/// </summary>
	[JsonNumberHandling(JsonNumberHandling.WriteAsString)]
	public long ParentId { get; set; }

	/// <summary>
	/// 节点名称
	/// </summary>
	public string Label { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	public int Weight { get; set; }

	/// <summary>
	/// 菜单类型（M目录 C菜单 F按钮）
	/// </summary>
	public string MenuType { get; set; }

	/// <summary>
	/// 菜单图标
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	/// 显示状态（0显示 1隐藏）
	/// </summary>
	public string Visible { get; set; }

	/// <summary>
	/// 菜单状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 是否禁用
	/// </summary>
	public bool Disabled { get; set; }

	/// <summary>
	/// 子节点
	/// </summary>
	public List<MenuTreeSelectNode> Children { get; set; } = [];
}
