using Autofac.Annotation;
using FreeRedis;
using sevencat.common;
using sevencat.ruoyi.common.constant;
using sevencat.ruoyi.common.entity;
using sevencat.ruoyi.sys.dto;
using ZiggyCreatures.Caching.Fusion;

namespace sevencat.ruoyi.sys.service;

/// <summary>
/// 在线用户监控业务层（对应 Java 的 <c>SysUserOnlineController</c> 内联的 RedisUtils / StpUtil 逻辑）
/// </summary>
/// <remarks>
/// Java 端在线会话存放在 Redis 的 <c>online_tokens:{token}</c>（<c>UserOnlineDTO</c>），token 生命周期由 Sa-Token 维护；
/// C# 端登录态存放在 FusionCache 的 <c>global:user_token:{token}</c>（<c>LoginUser</c>），因此对应关系调整为：
/// 1. 会话对象由 <c>UserOnlineDTO</c> 改为 <c>LoginUser</c>，读取后映射为 <c>UserOnlineDTO</c> 返回；
/// 2. 在线列表改为扫描 Redis 中 <c>global:user_token:*</c> 的 key（对应 Java 的 <c>RedisUtils.keys</c>）；
/// 3. token 是否有效直接以缓存中是否仍存在会话为准（Sa-Token 的 <c>getTokenActiveTimeoutByToken</c> 判断无需实现）；
/// 4. 强退即移除对应的会话缓存（对应 Java 的 <c>StpUtil.kickoutByTokenValue</c>）。
/// </remarks>
[Component]
public class SysUserOnlineService(
	IFusionCache cache,
	RedisClient redisClient,
	LoginService loginService,
	SseManager sseManager)
{
	/// <summary>
	/// 在线会话缓存 key 前缀
	/// </summary>
	private const string OnlineTokenKey = GlobalConstants.USER_TOKEN_KEY;

	/// <summary>
	/// 获取在线用户监控列表，并按 IP 或用户名条件过滤当前有效会话（对应 Java 的 <c>list</c>）。
	/// </summary>
	/// <param name="ipaddr">IP地址</param>
	/// <param name="userName">用户名</param>
	/// <returns>在线用户列表</returns>
	public async Task<List<UserOnlineDTO>> SelectOnlineList(string ipaddr, string userName)
	{
		var sessions = await GetOnlineSessions();
		IEnumerable<LoginUser> filtered = sessions;
		// 对应 Java 中 ipaddr / userName 同时或单一非空的过滤分支
		if (ipaddr.IsNotNullOrWhiteSpace() && userName.IsNotNullOrWhiteSpace())
		{
			filtered = sessions.Where(x => ipaddr == x.Ipaddr && userName == x.Username);
		}
		else if (ipaddr.IsNotNullOrWhiteSpace())
		{
			filtered = sessions.Where(x => ipaddr == x.Ipaddr);
		}
		else if (userName.IsNotNullOrWhiteSpace())
		{
			filtered = sessions.Where(x => userName == x.Username);
		}

		return ToReversedDtoList(filtered);
	}

	/// <summary>
	/// 获取当前登录用户的在线设备列表，仅返回当前账号仍有效的 token 会话（对应 Java 的 <c>getInfo</c>）。
	/// </summary>
	/// <returns>当前用户在线设备列表；未登录时返回 null（对应 Java 的 NotLoginException）</returns>
	public async Task<List<UserOnlineDTO>> SelectMyOnlineList()
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser == null)
		{
			return null;
		}

		// 对应 Java 的 StpUtil.getTokenValueListByLoginId(StpUtil.getLoginIdAsString())
		var loginId = loginUser.GetLoginId();
		var sessions = await GetOnlineSessions();
		return ToReversedDtoList(sessions.Where(x => x.GetLoginId() == loginId));
	}

	/// <summary>
	/// 按 token 强制用户下线，适用于管理员踢除异常会话（对应 Java 的 <c>forceLogout</c>）。
	/// </summary>
	/// <param name="tokenId">token值</param>
	public async Task ForceLogout(string tokenId)
	{
		// 先取出会话，用于定位对应的 SSE 连接后再清除缓存
		var target = await cache.GetOrDefaultAsync<LoginUser>(OnlineTokenKey + tokenId);
		await cache.RemoveAsync(OnlineTokenKey + tokenId);
		if (target?.UserId is long userId)
		{
			// 通知并关闭被强退用户的 SSE 连接
			await sseManager.KickOff(userId, tokenId);
		}
	}

	/// <summary>
	/// 强退当前账号下指定在线设备，避免误踢其他账号的会话（对应 Java 的 <c>remove</c>）。
	/// </summary>
	/// <param name="tokenId">token值</param>
	public async Task RemoveMySelf(string tokenId)
	{
		var loginUser = await loginService.GetLoginUser();
		if (loginUser == null)
		{
			return;
		}

		var target = await cache.GetOrDefaultAsync<LoginUser>(OnlineTokenKey + tokenId);
		if (target == null)
		{
			return;
		}

		// 只有目标会话属于当前登录账号时才允许强退
		if (target.GetLoginId() == loginUser.GetLoginId())
		{
			await cache.RemoveAsync(OnlineTokenKey + tokenId);
			if (target.UserId is long userId)
			{
				// 通知并关闭被强退设备的 SSE 连接
				await sseManager.KickOff(userId, tokenId);
			}
		}
	}

	/// <summary>
	/// 扫描 Redis 中的在线会话（对应 Java 的 <c>RedisUtils.keys(ONLINE_TOKEN_KEY + "*")</c> 逐个取缓存）。
	/// </summary>
	/// <returns>仍有效的登录会话列表</returns>
	private async Task<List<LoginUser>> GetOnlineSessions()
	{
		// FusionCache 写入分布式缓存时会在 key 上追加 wire format 版本等修饰符（如 v2:），
		// 故此处用通配符前后匹配，避免依赖其内部 key 格式
		var keys = await redisClient.KeysAsync("*" + OnlineTokenKey + "*");
		var tasks = keys.Select(async key =>
		{
			var token = ExtractToken(key);
			if (token == null)
			{
				return null;
			}

			return await cache.GetOrDefaultAsync<LoginUser>(OnlineTokenKey + token);
		});
		var sessions = await Task.WhenAll(tasks);
		// 已过期的会话（缓存已被清理）直接跳过，对应 Java 中对 null 的剔除
		return sessions.Where(x => x != null).ToList();
	}

	/// <summary>
	/// 从 Redis key 中截取 token 部分（key 形如 <c>[修饰符:]global:user_token:{token}[:修饰符]</c>）。
	/// </summary>
	/// <param name="key">Redis key</param>
	/// <returns>token，无法识别时返回 null</returns>
	private static string ExtractToken(string key)
	{
		var idx = key.IndexOf(OnlineTokenKey, StringComparison.Ordinal);
		if (idx < 0)
		{
			return null;
		}

		var start = idx + OnlineTokenKey.Length;
		var end = key.IndexOf(':', start);
		var token = end < 0 ? key[start..] : key[start..end];
		return token.Length == 0 ? null : token;
	}

	/// <summary>
	/// 将登录会话映射为在线信息并反转顺序（对应 Java 的 <c>BeanUtil.copyToList</c> + <c>Collections.reverse</c>）。
	/// </summary>
	/// <param name="sessions">登录会话集合</param>
	/// <returns>在线信息列表</returns>
	private static List<UserOnlineDTO> ToReversedDtoList(IEnumerable<LoginUser> sessions)
	{
		var list = sessions.Select(ToDto).ToList();
		// Java 端对列表做了 reverse（后登录的排前面）
		list.Reverse();
		return list;
	}

	/// <summary>
	/// 登录会话转在线信息对象（对应 Java 的 <c>BeanUtil.copyToList(userOnlineDTOList, SysUserOnline.class)</c>）。
	/// </summary>
	/// <param name="user">登录会话</param>
	/// <returns>在线信息对象</returns>
	private static UserOnlineDTO ToDto(LoginUser user)
	{
		return new UserOnlineDTO
		{
			TokenId = user.Token,
			DeptName = user.DeptName,
			UserName = user.Username,
			ClientKey = user.ClientKey,
			DeviceType = user.DeviceType,
			Ipaddr = user.Ipaddr,
			LoginLocation = user.LoginLocation,
			Browser = user.Browser,
			Os = user.Os,
			LoginTime = user.LoginTime,
		};
	}
}
