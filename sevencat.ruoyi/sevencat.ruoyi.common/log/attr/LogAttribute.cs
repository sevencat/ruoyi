using sevencat.ruoyi.common.enums;

namespace sevencat.ruoyi.common.log.attr;

/// <summary>
/// 操作日志记录特性（对应 Java 的 <c>com.ruoyi.common.annotation.Log</c>）
/// </summary>
/// <remarks>
/// Java 端由 AOP 切面 <c>LogAspect</c> 读取该注解并落库；C# 端由
/// <c>sevencat.ruoyi.sys.util.OperLogFilter</c> 读取该特性并落库，
/// 过滤器在 <c>Program.cs</c> 中全局注册，因此只要给 action 打上 <c>[Log]</c> 即可生效。
/// <para>用法：<c>[Log("岗位管理", BusinessTypeEnum.Insert)]</c></para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class LogAttribute(string title, BusinessTypeEnum businessType = BusinessTypeEnum.Other) : Attribute
{
	/// <summary>
	/// 模块标题
	/// </summary>
	public string Title { get; } = title;

	/// <summary>
	/// 业务类型
	/// </summary>
	public BusinessTypeEnum BusinessType { get; } = businessType;

	/// <summary>
	/// 操作人类别
	/// </summary>
	public OperatorTypeEnum OperatorType { get; set; } = OperatorTypeEnum.Manage;

	/// <summary>
	/// 是否保存请求的参数
	/// </summary>
	public bool IsSaveRequestData { get; set; } = true;

	/// <summary>
	/// 是否保存响应的参数
	/// </summary>
	public bool IsSaveResponseData { get; set; } = true;
}
