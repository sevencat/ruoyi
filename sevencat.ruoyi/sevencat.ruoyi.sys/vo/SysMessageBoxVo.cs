namespace sevencat.ruoyi.sys.vo;

/// <summary>
/// 消息盒子视图对象
/// </summary>
public class SysMessageBoxVo
{
	/// <summary>
	/// 系统消息
	/// </summary>
	public List<SysMessageVo> SystemList { get; set; } = [];

	/// <summary>
	/// 通知公告消息
	/// </summary>
	public List<SysMessageVo> NoticeList { get; set; } = [];

	/// <summary>
	/// 工作流消息
	/// </summary>
	public List<SysMessageVo> WorkflowList { get; set; } = [];
}
