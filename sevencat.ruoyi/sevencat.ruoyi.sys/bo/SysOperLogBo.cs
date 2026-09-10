namespace sevencat.ruoyi.sys.bo;

/// <summary>
/// 操作日志记录业务对象 sys_oper_log
/// </summary>
// Java 原注解 @Data：C# 端使用自动属性（无需 Lombok）
// Java 原注解 @AutoMappers({@AutoMapper(target = SysOperLog.class, reverseConvertGenerate = false), @AutoMapper(target = OperLogEvent.class)})：
// C# 端无对应映射框架注解，且 OperLogEvent 尚无等价类型，实体/BO 转换请用 Mapster 或手工映射
// Java 原注解 @Serial + serialVersionUID：C# 无等价物，已省略
public class SysOperLogBo
{
	/// <summary>
	/// 日志主键
	/// </summary>
	public long? OperId { get; set; }

	/// <summary>
	/// 模块标题
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// 业务类型（0其它 1新增 2修改 3删除）
	/// </summary>
	public int? BusinessType { get; set; }

	/// <summary>
	/// 业务类型数组
	/// </summary>
	public int[] BusinessTypes { get; set; }

	/// <summary>
	/// 方法名称
	/// </summary>
	public string Method { get; set; }

	/// <summary>
	/// 请求方式
	/// </summary>
	public string RequestMethod { get; set; }

	/// <summary>
	/// 操作类别（0其它 1后台用户 2手机端用户）
	/// </summary>
	public int? OperatorType { get; set; }

	/// <summary>
	/// 操作人员
	/// </summary>
	public string OperName { get; set; }

	/// <summary>
	/// 操作用户ID
	/// </summary>
	public long? UserId { get; set; }

	/// <summary>
	/// 操作部门ID
	/// </summary>
	public long? DeptId { get; set; }

	/// <summary>
	/// 部门名称
	/// </summary>
	public string DeptName { get; set; }

	/// <summary>
	/// 客户端
	/// </summary>
	public string ClientKey { get; set; }

	/// <summary>
	/// 设备类型
	/// </summary>
	public string DeviceType { get; set; }

	/// <summary>
	/// 浏览器类型
	/// </summary>
	public string Browser { get; set; }

	/// <summary>
	/// 操作系统
	/// </summary>
	public string Os { get; set; }

	/// <summary>
	/// 请求URL
	/// </summary>
	public string OperUrl { get; set; }

	/// <summary>
	/// 主机地址
	/// </summary>
	public string OperIp { get; set; }

	/// <summary>
	/// 操作地点
	/// </summary>
	public string OperLocation { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public string OperParam { get; set; }

	/// <summary>
	/// 返回参数
	/// </summary>
	public string JsonResult { get; set; }

	/// <summary>
	/// 操作状态（0正常 1异常）
	/// </summary>
	public int? Status { get; set; }

	/// <summary>
	/// 错误消息
	/// </summary>
	public string ErrorMsg { get; set; }

	/// <summary>
	/// 操作时间
	/// </summary>
	public DateTime? OperTime { get; set; }

	/// <summary>
	/// 消耗时间
	/// </summary>
	public long? CostTime { get; set; }

	/// <summary>
	/// 请求参数
	/// </summary>
	public Dictionary<string, object> Params { get; set; } = new();
}
