namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 对象存储配置视图对象 sys_oss_config
/// </summary>
// Java 原注解 @AutoMapper(target = SysOssConfig.class)：C# 端无对应映射框架注解，实体/VO 转换请用 Mapster 或手工映射
// Java 原注解 @ExcelIgnoreUnannotated：MiniExcel 无「只导出带注解属性」的类级开关，默认导出所有公开属性，
// 如需排除某个属性请单独打 [ExcelIgnore]
public class SysOssConfigVo
{
	/// <summary>
	/// 主键
	/// </summary>
	public long? OssConfigId { get; set; }

	/// <summary>
	/// 配置key
	/// </summary>
	public string ConfigKey { get; set; }

	/// <summary>
	/// accessKey
	/// </summary>
	public string AccessKey { get; set; }

	/// <summary>
	/// 秘钥
	/// </summary>
	public string SecretKey { get; set; }

	/// <summary>
	/// 桶名称
	/// </summary>
	public string BucketName { get; set; }

	/// <summary>
	/// 前缀
	/// </summary>
	public string Prefix { get; set; }

	/// <summary>
	/// 访问站点
	/// </summary>
	public string Endpoint { get; set; }

	/// <summary>
	/// 自定义域名
	/// </summary>
	public string DomainUrl { get; set; }

	/// <summary>
	/// 是否https（Y=是,N=否）
	/// </summary>
	public string IsHttps { get; set; }

	/// <summary>
	/// 域
	/// </summary>
	public string Region { get; set; }

	/// <summary>
	/// 是否默认（Y=是,N=否）
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	/// 扩展字段
	/// </summary>
	public string Ext1 { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }

	/// <summary>
	/// 桶权限类型(0private 1public 2custom)
	/// </summary>
	public string AccessPolicy { get; set; }
}
