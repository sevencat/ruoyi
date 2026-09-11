using FreeSql.DataAnnotations;
using sevencat.ruoyi.common.db.attr;

namespace sevencat.ruoyi.sys.entity.db;

/// <summary>
/// 操作日志记录表 sys_oper_log
/// </summary>
/// <remarks>
/// 表中无 create_by / create_time 等审计字段，故不继承
/// <see cref="sevencat.ruoyi.common.entity.db.TBaseEntity"/>。
/// </remarks>
[Table(Name = "sys_oper_log")]
[Index("idx_sys_oper_log_bt", "BusinessType")]
[Index("idx_sys_oper_log_uid", "UserId")]
[Index("idx_sys_oper_log_s", "Status")]
[Index("idx_sys_oper_log_ot", "OperTime")]
public class TSysOperLog
{
	/// <summary>
	/// 日志主键
	/// </summary>
	[Column(Name = "oper_id", IsPrimary = true)]
	public long OperId { get; set; }

	/// <summary>
	/// 模块标题
	/// </summary>
	[Column(Name = "title", StringLength = 50, IsNullable = true)]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// 业务类型（0其它 1新增 2修改 3删除）
	/// </summary>
	[Column(Name = "business_type", IsNullable = true)]
	public int? BusinessType { get; set; }

	/// <summary>
	/// 方法名称
	/// </summary>
	[Column(Name = "method", StringLength = 100, IsNullable = true)]
	public string Method { get; set; } = string.Empty;

	/// <summary>
	/// 请求方式
	/// </summary>
	[Column(Name = "request_method", StringLength = 10, IsNullable = true)]
	public string RequestMethod { get; set; } = string.Empty;

	/// <summary>
	/// 操作类别（0其它 1后台用户 2手机端用户）
	/// </summary>
	[Column(Name = "operator_type", IsNullable = true)]
	public int? OperatorType { get; set; }

	/// <summary>
	/// 操作人员
	/// </summary>
	[Column(Name = "oper_name", StringLength = 50, IsNullable = true)]
	public string OperName { get; set; } = string.Empty;

	/// <summary>
	/// 操作用户ID
	/// </summary>
	[Column(Name = "user_id", IsNullable = true)]
	public long? UserId { get; set; }

	/// <summary>
	/// 操作部门ID
	/// </summary>
	[Column(Name = "dept_id", IsNullable = true)]
	public long? DeptId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	[Column(Name = "dept_name", StringLength = 50, IsNullable = true)]
	public string DeptName { get; set; } = string.Empty;

	/// <summary>
	/// 客户端
	/// </summary>
	[Column(Name = "client_key", StringLength = 32, IsNullable = true)]
	public string ClientKey { get; set; } = string.Empty;

	/// <summary>
	/// 设备类型
	/// </summary>
	[Column(Name = "device_type", StringLength = 32, IsNullable = true)]
	public string DeviceType { get; set; } = string.Empty;

	/// <summary>
	/// 浏览器类型
	/// </summary>
	[Column(Name = "browser", StringLength = 50, IsNullable = true)]
	public string Browser { get; set; } = string.Empty;

	/// <summary>
	/// 操作系统
	/// </summary>
	[Column(Name = "os", StringLength = 50, IsNullable = true)]
	public string Os { get; set; } = string.Empty;

	/// <summary>
	/// 请求URL
	/// </summary>
	[Column(Name = "oper_url", StringLength = 255, IsNullable = true)]
	public string OperUrl { get; set; } = string.Empty;

	/// <summary>
	/// 主机地址
	/// </summary>
	[Column(Name = "oper_ip", StringLength = 128, IsNullable = true)]
	public string OperIp { get; set; } = string.Empty;

	/// <summary>
	/// 操作地点
	/// </summary>
	[Column(Name = "oper_location", StringLength = 255, IsNullable = true)]
	public string OperLocation { get; set; } = string.Empty;

	/// <summary>
	/// 请求参数
	/// </summary>
	[Column(Name = "oper_param", StringLength = 4000, IsNullable = true)]
	public string OperParam { get; set; } = string.Empty;

	/// <summary>
	/// 返回参数
	/// </summary>
	[Column(Name = "json_result", StringLength = 4000, IsNullable = true)]
	public string JsonResult { get; set; } = string.Empty;

	/// <summary>
	/// 操作状态（0正常 1异常）
	/// </summary>
	[Column(Name = "status", IsNullable = true)]
	public int? Status { get; set; }

	/// <summary>
	/// 错误消息
	/// </summary>
	[Column(Name = "error_msg", StringLength = 4000, IsNullable = true)]
	public string ErrorMsg { get; set; } = string.Empty;

	/// <summary>
	/// 操作时间
	/// </summary>
	[Column(Name = "oper_time", IsNullable = true)]
	public DateTime? OperTime { get; set; }

	/// <summary>
	/// 消耗时间
	/// </summary>
	[Column(Name = "cost_time", IsNullable = true)]
	public long? CostTime { get; set; }
}
