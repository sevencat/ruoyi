using System.Text.Json;
using Autofac.Annotation;
using FreeRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using sevencat.common;
using sevencat.ruoyi.config.impl;
using ZiggyCreatures.Caching.Fusion;

namespace sevencat.ruoyi.config;

[AutoConfiguration]
public class CacheConfig
{
	[Bean]
	public IMemoryCache CreateMemoryCache(IConfiguration cfg)
	{
		var opts = new MemoryCacheOptions();
		var limits = cfg.GetValue<int>("cache:sizelimit");
		if (limits > 0)
		{
			opts.SizeLimit = limits;
		}

		var memoryCache = new MemoryCache(opts);
		return memoryCache;
	}

	[Bean]
	public RedisClient CreateRedisClient(IConfiguration cfg)
	{
		var redisurl = cfg.GetValue<string>("cache:redisurl");
		var cli = new RedisClient(redisurl);
		cli.Serialize = obj => obj.ToJson();
		cli.Deserialize = (json, type) => JsonSerializer.Deserialize(json, type);
		return cli;
	}

	[Bean]
	public IDistributedCache CreateDCache(RedisClient cli)
	{
		var distributedCache = new DistributedCache(cli);
		return distributedCache;
	}

	[Bean]
	public IFusionCache CreateCache(IDistributedCache dc, IMemoryCache mc)
	{
		var cache = new FusionCache(new FusionCacheOptions(), mc);
		var serializer = new FusionCacheSystemTextJsonSerializer();
		cache.SetupDistributedCache(dc, serializer);
		return cache;
	}
}
