namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 菜单权限业务对象 sys_menu
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMapper(target = SysMenu.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@Size/@NotNull/@Pattern/@JsonPattern 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysMenuBo
{
	/// <summary>
	/// 菜单ID
	/// </summary>
	public long? MenuId { get; set; }

	/// <summary>
	/// 父菜单ID
	/// </summary>
	public long? ParentId { get; set; }

	/// <summary>
	/// 菜单名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "菜单名称不能为空")
	// Java 原注解 @Size(min = 0, max = 50, message = "菜单名称长度不能超过{max}个字符")
	public string MenuName { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	// Java 原注解 @NotNull(message = "显示顺序不能为空")
	public int? OrderNum { get; set; }

	/// <summary>
	/// 路由地址
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 200, message = "路由地址不能超过{max}个字符")
	public string Path { get; set; }

	/// <summary>
	/// 组件路径
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 200, message = "组件路径不能超过{max}个字符")
	public string Component { get; set; }

	/// <summary>
	/// 路由参数
	/// </summary>
	// Java 原注解 @JsonPattern(type = JsonType.OBJECT, message = "路由参数必须符合JSON格式")：C# 端无等价校验注解
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
	// Java 原注解 @NotBlank(message = "菜单类型不能为空")
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
	// Java 原注解 @JsonInclude(JsonInclude.Include.NON_NULL)：C# 端序列化时忽略 null 需在序列化配置中设置 DefaultIgnoreCondition = WhenWritingNull
	// Java 原注解 @Size(min = 0, max = 100, message = "权限标识长度不能超过{max}个字符")
	// Java 原注解 @Pattern(regexp = RegexConstants.PERMISSION_STRING, message = "权限标识必须符合 tool:build:list 格式")
	public string Perms { get; set; }

	/// <summary>
	/// 菜单图标
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	/// 激活菜单路径
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 255, message = "激活菜单路径长度不能超过{max}个字符")
	public string ActiveMenu { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	// Java 原注解 @Size(min = 0, max = 2000, message = "扩展字段长度不能超过{max}个字符")
	public string Ext { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }
}
