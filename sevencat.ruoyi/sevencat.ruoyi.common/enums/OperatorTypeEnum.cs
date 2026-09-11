namespace sevencat.ruoyi.common.enums;

/// <summary>
/// 操作人类别（对应 Java 的 <c>com.ruoyi.common.enums.OperatorType</c>）
/// </summary>
/// <remarks>
/// 枚举值即 <c>sys_oper_log.operator_type</c> 存储的数值，不可调整。
/// </remarks>
public enum OperatorTypeEnum
{
	/// <summary>
	/// 其它
	/// </summary>
	Other = 0,

	/// <summary>
	/// 后台用户
	/// </summary>
	Manage = 1,

	/// <summary>
	/// 手机端用户
	/// </summary>
	Mobile = 2
}
