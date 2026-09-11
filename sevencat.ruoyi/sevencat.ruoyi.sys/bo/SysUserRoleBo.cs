namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 用户和角色关联业务对象 sys_user_role
/// </summary>
/// <remarks>
/// 对应 Java 中直接以领域对象 <c>SysUserRole</c> 作为「取消授权用户」接口的入参；
/// C# 端按控制器约定统一使用 BO 承接请求体。
/// </remarks>
public class SysUserRoleBo
{
	/// <summary>
	/// 用户ID
	/// </summary>
	public long UserId { get; set; }

	/// <summary>
	/// 角色ID
	/// </summary>
	public long RoleId { get; set; }
}
