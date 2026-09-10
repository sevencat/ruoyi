using sevencat.ruoyi.sys.constant;

namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 角色信息业务对象 sys_role
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @NoArgsConstructor：C# 隐式提供无参构造函数，无需额外声明
// Java 原注解 @AutoMapper(target = SysRole.class, reverseConvertGenerate = false)：C# 端无对应映射框架注解，实体/BO 转换请用 Mapster 或手工映射
// Java 校验注解（@NotBlank/@NotNull/@Size 等）：C# 端无等价校验管线，已按字段以注释保留，如需校验请自行使用 DataAnnotations/FluentValidation
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysRoleBo
{
	/// <summary>
	/// 角色ID
	/// </summary>
	public long? RoleId { get; set; }

	/// <summary>
	/// 角色名称
	/// </summary>
	// Java 原注解 @NotBlank(message = "角色名称不能为空")
	// Java 原注解 @Size(min = 0, max = 30, message = "角色名称长度不能超过{max}个字符")
	public string RoleName { get; set; }

	/// <summary>
	/// 角色权限字符串
	/// </summary>
	// Java 原注解 @NotBlank(message = "角色权限字符串不能为空")
	// Java 原注解 @Size(min = 0, max = 100, message = "权限字符长度不能超过{max}个字符")
	public string RoleKey { get; set; }

	/// <summary>
	/// 显示顺序
	/// </summary>
	// Java 原注解 @NotNull(message = "显示顺序不能为空")
	public int? RoleSort { get; set; }

	/// <summary>
	/// 数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5：仅本人数据权限 6：部门及以下或本人数据权限）
	/// </summary>
	public string DataScope { get; set; }

	/// <summary>
	/// 菜单树选择项是否关联显示
	/// </summary>
	public bool? MenuCheckStrictly { get; set; }

	/// <summary>
	/// 部门树选择项是否关联显示
	/// </summary>
	public bool? DeptCheckStrictly { get; set; }

	/// <summary>
	/// 角色状态（0正常 1停用）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 菜单组
	/// </summary>
	public long[] MenuIds { get; set; }

	/// <summary>
	/// 部门组（数据权限）
	/// </summary>
	public long[] DeptIds { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();

	/// <summary>
	/// 判断当前角色是否为超级管理员角色。
	/// </summary>
	/// <returns>true 是超级管理员角色 false 不是超级管理员角色</returns>
	public bool IsSuperAdmin()
	{
		return RoleId == SystemConstants.SUPER_ADMIN_ROLE_ID;
	}
}
