namespace sevencat.ruoyi.sys.entity.dto;

/// <summary>
/// 任务受让人
/// </summary>
public class TaskAssigneeDTO
{
	/// <summary>
	/// 总大小
	/// </summary>
	public long? Total { get; set; } = 0L;

	/// <summary>
	/// 受让人列表（属性名与 Java 端保持一致，序列化后为 list）
	/// </summary>
	public List<TaskHandler> List { get; set; }

	/// <summary>
	/// 创建任务受让人分页结果
	/// </summary>
	/// <param name="total">总大小</param>
	/// <param name="list">受让人列表</param>
	public TaskAssigneeDTO(long? total, List<TaskHandler> list)
	{
		Total = total;
		List = list;
	}

	/// <summary>
	/// 创建任务受让人分页结果
	/// </summary>
	public TaskAssigneeDTO()
	{
	}

	/// <summary>
	/// 将源列表转换为 TaskHandler 列表
	/// </summary>
	/// <typeparam name="T">通用类型</typeparam>
	/// <param name="sourceCollection">待转换的源列表</param>
	/// <param name="storageId">提取 storageId 的函数</param>
	/// <param name="handlerCode">提取 handlerCode 的函数</param>
	/// <param name="handlerName">提取 handlerName 的函数</param>
	/// <param name="groupName">提取 groupName 的函数</param>
	/// <param name="createTimeMapper">提取 createTime 的函数</param>
	/// <returns>转换后的 TaskHandler 列表</returns>
	public static List<TaskHandler> ConvertToHandlerList<T>(
		IEnumerable<T> sourceCollection,
		Func<T, string> storageId,
		Func<T, string> handlerCode,
		Func<T, string> handlerName,
		Func<T, string> groupName,
		Func<T, DateTime?> createTimeMapper)
	{
		return sourceCollection.Select(item => new TaskHandler(
			storageId(item),
			handlerCode(item),
			handlerName(item),
			groupName(item),
			createTimeMapper(item))).ToList();
	}

	/// <summary>
	/// 任务受让人明细对象
	/// </summary>
	public class TaskHandler
	{
		/// <summary>
		/// 主键
		/// </summary>
		public string StorageId { get; set; }

		/// <summary>
		/// 权限编码
		/// </summary>
		public string HandlerCode { get; set; }

		/// <summary>
		/// 权限名称
		/// </summary>
		public string HandlerName { get; set; }

		/// <summary>
		/// 权限分组
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime? CreateTime { get; set; }

		/// <summary>
		/// 任务受让人明细
		/// </summary>
		public TaskHandler()
		{
		}

		/// <summary>
		/// 任务受让人明细
		/// </summary>
		/// <param name="storageId">主键</param>
		/// <param name="handlerCode">权限编码</param>
		/// <param name="handlerName">权限名称</param>
		/// <param name="groupName">权限分组</param>
		/// <param name="createTime">创建时间</param>
		public TaskHandler(string storageId, string handlerCode, string handlerName, string groupName, DateTime? createTime)
		{
			StorageId = storageId;
			HandlerCode = handlerCode;
			HandlerName = handlerName;
			GroupName = groupName;
			CreateTime = createTime;
		}
	}
}
