namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 路由配置信息
/// </summary>
// Java 原注解 @JsonInclude(JsonInclude.Include.NON_EMPTY)：System.Text.Json 无类级「忽略空值」开关，
// 如需实现同等效果，可在序列化时全局配置 DefaultIgnoreCondition，
// 或对单个属性打 [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull/WhenWritingDefault)]
public class RouterVo
{
	/// <summary>
	/// 路由名字
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// 路由地址
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// 是否隐藏路由，当设置 true 的时候该路由不会再侧边栏出现
	/// </summary>
	public bool? Hidden { get; set; }

	/// <summary>
	/// 重定向地址，当设置 noRedirect 的时候该路由在面包屑导航中不可被点击
	/// </summary>
	public string Redirect { get; set; }

	/// <summary>
	/// 组件地址
	/// </summary>
	public string Component { get; set; }

	/// <summary>
	/// 路由参数：如 {"id": 1, "name": "ry"}
	/// </summary>
	public string Query { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	public string Ext { get; set; }

	/// <summary>
	/// 当你一个路由下面的 children 声明的路由大于1个时，自动会变成嵌套的模式--如组件页面
	/// </summary>
	public bool? AlwaysShow { get; set; }

	/// <summary>
	/// 其他元素
	/// </summary>
	public MetaVo Meta { get; set; }

	/// <summary>
	/// 子路由
	/// </summary>
	public List<RouterVo> Children { get; set; }
}
