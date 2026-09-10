namespace sevencat.ruoyi.sys.constant;

/// <summary>
/// 系统通用常量
/// </summary>
public static class SystemConstants
{
	/// <summary>
	/// 正常状态
	/// </summary>
	public const string NORMAL = "0";

	/// <summary>
	/// 异常状态
	/// </summary>
	public const string DISABLE = "1";

	/// <summary>
	/// 是
	/// </summary>
	public const string YES = "Y";

	/// <summary>
	/// 否
	/// </summary>
	public const string NO = "N";

	/// <summary>
	/// 菜单类型（目录）
	/// </summary>
	public const string TYPE_DIR = "M";

	/// <summary>
	/// 菜单类型（菜单）
	/// </summary>
	public const string TYPE_MENU = "C";

	/// <summary>
	/// 菜单类型（按钮）
	/// </summary>
	public const string TYPE_BUTTON = "F";

	/// <summary>
	/// Layout组件标识
	/// </summary>
	public const string LAYOUT = "Layout";

	/// <summary>
	/// ParentView组件标识
	/// </summary>
	public const string PARENT_VIEW = "ParentView";

	/// <summary>
	/// InnerLink组件标识
	/// </summary>
	public const string INNER_LINK = "InnerLink";

	/// <summary>
	/// 超级管理员用户ID
	/// </summary>
	public const long SUPER_ADMIN_USER_ID = 1761100000000000001L;

	/// <summary>
	/// 超级管理员角色ID
	/// </summary>
	public const long SUPER_ADMIN_ROLE_ID = 1761300000000000001L;

	/// <summary>
	/// 超级管理员角色 roleKey
	/// </summary>
	public const string SUPER_ADMIN_ROLE_KEY = "superadmin";

	/// <summary>
	/// 根部门祖级列表
	/// </summary>
	public const string ROOT_DEPT_ANCESTORS = "0";

	/// <summary>
	/// 默认部门 ID
	/// </summary>
	public const long DEFAULT_DEPT_ID = 1761000000000000100L;

	/// <summary>
	/// 排除敏感属性字段
	/// </summary>
	public static readonly string[] EXCLUDE_PROPERTIES =
	[
		"password",
		"oldPassword",
		"newPassword",
		"confirmPassword"
	];
}
