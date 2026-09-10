namespace sevencat.ruoyi.common.security.attr;

[AttributeUsage(AttributeTargets.Method)]
public class SaCheckPermissionAttribute(params string[] perms) : Attribute
{
}