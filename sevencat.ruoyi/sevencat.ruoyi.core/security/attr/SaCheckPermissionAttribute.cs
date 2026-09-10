namespace sevencat.ruoyi.core.security.attr;

[AttributeUsage(AttributeTargets.Method)]
public class SaCheckPermissionAttribute(params string[] perms) : Attribute
{
}