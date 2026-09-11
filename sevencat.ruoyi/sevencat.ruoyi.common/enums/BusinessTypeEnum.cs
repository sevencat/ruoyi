namespace sevencat.ruoyi.common.enums;

/// <summary>
/// 业务操作类型（对应 Java 的 <c>com.ruoyi.common.enums.BusinessType</c>）
/// </summary>
/// <remarks>
/// 枚举值即 <c>sys_oper_log.business_type</c> 存储的数值，不可调整。
/// </remarks>
public enum BusinessTypeEnum
{
	/// <summary>
	/// 其它
	/// </summary>
	Other = 0,

	/// <summary>
	/// 新增
	/// </summary>
	Insert = 1,

	/// <summary>
	/// 修改
	/// </summary>
	Update = 2,

	/// <summary>
	/// 删除
	/// </summary>
	Delete = 3,

	/// <summary>
	/// 授权
	/// </summary>
	Grant = 4,

	/// <summary>
	/// 导出
	/// </summary>
	Export = 5,

	/// <summary>
	/// 导入
	/// </summary>
	Import = 6,

	/// <summary>
	/// 强退
	/// </summary>
	Force = 7,

	/// <summary>
	/// 清空数据
	/// </summary>
	Clean = 8
}
