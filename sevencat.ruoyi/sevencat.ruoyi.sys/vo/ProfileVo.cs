namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 个人中心信息（对应 Java <c>SysProfileController</c> 内部的 record <c>ProfileVo</c>）
/// </summary>
/// <param name="User">用户信息（专用 VO，避免被脱敏）</param>
/// <param name="RoleGroup">用户所属角色组</param>
/// <param name="PostGroup">用户所属岗位组</param>
public record ProfileVo(ProfileUserVo User, string RoleGroup, string PostGroup);
