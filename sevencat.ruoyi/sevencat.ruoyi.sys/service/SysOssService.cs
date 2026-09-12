using System.Text.Json;
using Autofac.Annotation;
using FreeSql;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using sevencat.common;
using sevencat.ruoyi.common.db.util;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.common.exception;
using sevencat.ruoyi.common.oss;
using sevencat.ruoyi.common.util;
using sevencat.ruoyi.sys.bo;
using sevencat.ruoyi.sys.dto;
using sevencat.ruoyi.sys.entity.db;
using sevencat.ruoyi.sys.vo;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// OSS 对象存储业务层（对应 Java 的 <c>ISysOssService</c> / <c>SysOssServiceImpl</c>）
/// </summary>
/// <remarks>
/// 与 Java 实现的差异：
/// <list type="number">
/// <item>不再依赖 <c>sys_oss_config</c> 与 <c>OssFactory</c>，直接使用容器中已注入的 <see cref="IOssClient"/>；</item>
/// <item>桶名、服务商标识、对外访问地址前缀都由 <see cref="IOssClient"/> 提供
/// （MinIO 实现取配置 <c>minio:bucket</c> 与 <c>minio:url</c>），本层只关心对象键；</item>
/// <item>Java 的 <c>matchingUrl</c> 用于给私有桶生成带签名的临时地址，这里统一保存/返回直连地址，因此无需重写 URL。</item>
/// </list>
/// </remarks>
[Component]
public class SysOssService(IFreeSql fsql, IMapper mapper, IOssClient ossClient)
{
	/// <summary>
	/// JSON 序列化配置（camelCase，与 Java 序列化 ext1 的字段风格保持一致）
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

	/// <summary>
	/// 分页查询 OSS 列表（对应 Java 的 <c>queryPageList</c>）
	/// </summary>
	/// <param name="bo">查询条件</param>
	/// <param name="pageQuery">分页参数</param>
	/// <returns>OSS 分页列表（已回填上传人名称）</returns>
	public async Task<PageResult<SysOssVo>> SelectPageOssList(SysOssBo bo, PageQuery2 pageQuery)
	{
		var page = await BuildOssQuery(bo).ToPage(pageQuery);
		var result = page.MapTo<SysOssVo>(mapper);
		await FillCreateByName(result.Rows);
		return result;
	}

	/// <summary>
	/// 按主键集合查询 OSS（对应 Java 的 <c>listByIds</c>）
	/// </summary>
	/// <param name="ossIds">对象存储主键集合</param>
	/// <returns>OSS 集合，顺序与入参一致（已回填上传人名称）</returns>
	public async Task<List<SysOssVo>> ListByIds(List<long> ossIds)
	{
		if (ossIds == null || ossIds.Count == 0)
		{
			return [];
		}

		var rows = await fsql.Select<TSysOss>()
			.Where(x => ossIds.Contains(x.OssId))
			.ToListAsync();
		var voMap = rows.ToDictionary(x => x.OssId, x => x.MapTo<SysOssVo>(mapper));

		var list = new List<SysOssVo>();
		foreach (var ossId in ossIds)
		{
			// Java 侧并发查询后按入参顺序组装，这里保持同样的顺序
			if (voMap.TryGetValue(ossId, out var vo))
			{
				list.Add(vo);
			}
		}

		await FillCreateByName(list);
		return list;
	}

	/// <summary>
	/// 查询 OSS 详情（对应 Java 的 <c>getById</c>）
	/// </summary>
	/// <param name="ossId">对象存储主键</param>
	/// <returns>OSS 详情（已回填上传人名称）；不存在时返回 null</returns>
	public async Task<SysOssVo> GetById(long ossId)
	{
		var row = await fsql.Select<TSysOss>()
			.Where(x => x.OssId == ossId)
			.FirstAsync();
		if (row == null)
		{
			return null;
		}

		var vo = row.MapTo<SysOssVo>(mapper);
		await FillCreateByName([vo]);
		return vo;
	}

	/// <summary>
	/// 上传文件（对应 Java 的 <c>upload</c>）
	/// </summary>
	/// <param name="file">上传的文件</param>
	/// <param name="ossExt">附件扩展信息，可为空</param>
	/// <returns>上传后的 OSS 信息，其中 <c>FileName</c> 为对象键、<c>Url</c> 为访问地址</returns>
	/// <exception cref="ServiceException">文件为空时抛出</exception>
	public async Task<SysOssVo> Upload(IFormFile file, SysOssExt ossExt)
	{
		if (file == null || file.Length == 0)
		{
			throw new ServiceException("上传文件不能为空");
		}

		var originalName = Path.GetFileName(file.FileName);
		var contentType = file.ContentType;
		// 与 Java 的 substring(lastIndexOf(".")) 一致：保留前导点，如 .png
		var suffix = Path.GetExtension(originalName);
		var fileName = BuildPathKey(originalName);

		await using (var stream = file.OpenReadStream())
		{
			await ossClient.PutObjectAsync(fileName, stream, file.Length, contentType);
		}

		var ext = ossExt ?? new SysOssExt();
		ext.FileSize = file.Length;
		ext.ContentType = contentType;

		var oss = new TSysOss
		{
			FileName = fileName,
			OriginalName = originalName,
			FileSuffix = suffix,
			Url = BuildUrl(fileName),
			Ext1 = JsonSerializer.Serialize(ext, JsonOptions),
			Service = ossClient.Service,
		};
		await fsql.Insert(oss).ExecuteAffrowsAsync();
		return oss.MapTo<SysOssVo>(mapper);
	}

	/// <summary>
	/// 下载文件（对应 Java 的 <c>download</c>）
	/// </summary>
	/// <param name="ossId">对象存储主键</param>
	/// <returns>文件内容、MIME 类型与原始文件名</returns>
	/// <exception cref="ServiceException">数据不存在时抛出</exception>
	public async Task<SysOssDownloadFile> Download(long ossId)
	{
		var oss = await fsql.Select<TSysOss>()
			.Where(x => x.OssId == ossId)
			.FirstAsync();
		if (oss == null)
		{
			throw new ServiceException("文件数据不存在!");
		}

		using var buffer = new MemoryStream();
		await ossClient.GetObjectAsync(oss.FileName, stream => stream.CopyTo(buffer));

		return new SysOssDownloadFile(buffer.ToArray(), ResolveContentType(oss.Ext1),
			oss.OriginalName.IsNotNullOrWhiteSpace() ? oss.OriginalName : oss.FileName);
	}

	/// <summary>
	/// 批量删除 OSS 记录与对象（对应 Java 的 <c>deleteWithValidByIds</c>）
	/// </summary>
	/// <param name="ids">对象存储主键集合</param>
	/// <param name="isValid">Java 中用于是否校验的占位参数，此处保留签名以保证调用点一致</param>
	/// <returns>是否删除成功</returns>
	public async Task<bool> DeleteWithValidByIds(List<long> ids, bool isValid)
	{
		if (ids == null || ids.Count == 0)
		{
			return false;
		}

		var list = await fsql.Select<TSysOss>()
			.Where(x => ids.Contains(x.OssId))
			.ToListAsync();
		foreach (var oss in list)
		{
			await ossClient.RemoveObjectAsync(oss.FileName);
		}

		return await fsql.Delete<TSysOss>()
			.Where(x => ids.Contains(x.OssId))
			.ExecuteAffrowsAsync() > 0;
	}

	/// <summary>
	/// 构造 OSS 列表查询条件（对应 Java 的 <c>buildQueryWrapper</c>）
	/// </summary>
	/// <param name="bo">筛选条件</param>
	/// <returns>OSS 列表查询对象</returns>
	private ISelect<TSysOss> BuildOssQuery(SysOssBo bo)
	{
		return fsql.Select<TSysOss>()
			.WhereLike(bo.FileName, x => x.FileName)
			.WhereLike(bo.OriginalName, x => x.OriginalName)
			.WhereHasTextEq(bo.FileSuffix, x => x.FileSuffix)
			.WhereHasTextEq(bo.Url, x => x.Url)
			// Java 的时间区间参数名为 beginCreateTime / endCreateTime
			.WhereTimeRange(bo.Params, x => x.CreateTime, "beginCreateTime", "endCreateTime")
			.WhereNotNullEq(bo.CreateBy, x => x.CreateBy)
			.WhereHasTextEq(bo.Service, x => x.Service)
			.OrderBy(x => x.OssId);
	}

	/// <summary>
	/// 回填 OSS 列表的上传人名称（对应 Java 的 @Translation USER_ID_TO_NAME）
	/// </summary>
	/// <param name="rows">OSS 列表</param>
	private async Task FillCreateByName(List<SysOssVo> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		var userIds = rows.Where(x => x.CreateBy.HasValue)
			.Select(x => x.CreateBy.Value)
			.Distinct()
			.ToList();
		if (userIds.Count == 0)
		{
			return;
		}

		var users = await fsql.Select<TSysUser>()
			.Where(x => userIds.Contains(x.UserId))
			.ToListAsync(x => new { x.UserId, x.UserName });
		var userNames = users.ToDictionary(x => x.UserId, x => x.UserName);

		foreach (var row in rows)
		{
			if (row.CreateBy.HasValue && userNames.TryGetValue(row.CreateBy.Value, out var userName))
			{
				row.CreateByName = userName;
			}
		}
	}

	/// <summary>
	/// 生成对象键（对应 Java 的 <c>buildPathKey</c>）：<c>{yyyy/MM/dd}/{uuid}{后缀}</c>
	/// </summary>
	/// <param name="originalFileName">原始文件名</param>
	/// <returns>对象键</returns>
	private static string BuildPathKey(string originalFileName)
	{
		var datePath = DateTime.Now.ToString("yyyy/MM/dd");
		var suffix = Path.GetExtension(originalFileName);
		return $"{datePath}/{Guid.NewGuid():N}{suffix}";
	}

	/// <summary>
	/// 拼接对象的对外访问地址（<c>{客户端的访问前缀}/{桶名}/{对象键}</c>）
	/// </summary>
	/// <param name="fileName">对象键</param>
	/// <returns>访问地址</returns>
	private string BuildUrl(string fileName)
	{
		var baseUrl = ossClient.Url;
		if (baseUrl.IsNullOrWhiteSpace())
		{
			return $"/{ossClient.BucketName}/{fileName}";
		}

		baseUrl = baseUrl.TrimEnd('/');
		if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
		{
			baseUrl = $"http://{baseUrl}";
		}

		return $"{baseUrl}/{ossClient.BucketName}/{fileName}";
	}

	/// <summary>
	/// 从 ext1 中解析上传时记录的 MIME 类型，解析不出时回退为二进制流
	/// </summary>
	/// <param name="ext1">扩展字段 JSON</param>
	/// <returns>MIME 类型</returns>
	private static string ResolveContentType(string ext1)
	{
		if (ext1.IsNotNullOrWhiteSpace())
		{
			try
			{
				var ext = JsonSerializer.Deserialize<SysOssExt>(ext1, JsonOptions);
				if (ext?.ContentType.IsNotNullOrWhiteSpace() == true)
				{
					return ext.ContentType;
				}
			}
			catch (JsonException)
			{
				// ext1 非本系统写入的 JSON 时忽略，走默认类型
			}
		}

		return "application/octet-stream";
	}
}

/// <summary>
/// OSS 下载返回内容
/// </summary>
/// <param name="Data">文件字节内容</param>
/// <param name="ContentType">MIME 类型</param>
/// <param name="FileName">原始文件名，供 Content-Disposition 使用</param>
public record SysOssDownloadFile(byte[] Data, string ContentType, string FileName);