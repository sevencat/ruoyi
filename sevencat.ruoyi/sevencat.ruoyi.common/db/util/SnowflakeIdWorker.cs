namespace sevencat.ruoyi.common.db.util;

public class SnowflakeIdWorker : IIdGen
{
	// 基准时间戳 (可以改成项目启动的时间，比如 2026-01-01)
	private readonly long twepoch = 1767225600000L;

	// 机器标识与数据中心标识所占位数
	private readonly int workerIdBits = 5;
	private readonly int datacenterIdBits = 5;
	private readonly int sequenceBits = 12;

	// 支持的最大数值
	private readonly long maxWorkerId;
	private readonly long maxDatacenterId;

	// 移位偏移量
	private readonly int workerIdShift;
	private readonly int datacenterIdShift;
	private readonly int timestampLeftShift;
	private readonly long sequenceMask;

	private long workerId;
	private long datacenterId;
	private long sequence = 0L;
	private long lastTimestamp = -1L;

	private static readonly object lockObj = new object();

	public SnowflakeIdWorker(long workerId, long datacenterId)
	{
		maxWorkerId = -1L ^ (-1L << workerIdBits);
		maxDatacenterId = -1L ^ (-1L << datacenterIdBits);

		if (workerId > maxWorkerId || workerId < 0)
			throw new ArgumentException($"worker Id can't be greater than {maxWorkerId} or less than 0");
		if (datacenterId > maxDatacenterId || datacenterId < 0)
			throw new ArgumentException($"datacenter Id can't be greater than {maxDatacenterId} or less than 0");

		this.workerId = workerId;
		this.datacenterId = datacenterId;

		workerIdShift = sequenceBits;
		datacenterIdShift = sequenceBits + workerIdBits;
		timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
		sequenceMask = -1L ^ (-1L << sequenceBits);
	}

	public long NextId()
	{
		lock (lockObj)
		{
			long timestamp = TimeGen();

			if (timestamp < lastTimestamp)
				throw new Exception(
					$"Clock moved backwards. Refusing to generate id for {lastTimestamp - timestamp} milliseconds");

			if (lastTimestamp == timestamp)
			{
				sequence = (sequence + 1) & sequenceMask;
				if (sequence == 0)
				{
					timestamp = TilNextMillis(lastTimestamp);
				}
			}
			else
			{
				sequence = 0L;
			}

			lastTimestamp = timestamp;

			return ((timestamp - twepoch) << timestampLeftShift)
			       | (datacenterId << datacenterIdShift)
			       | (workerId << workerIdShift)
			       | sequence;
		}
	}

	private long TilNextMillis(long lastTimestamp)
	{
		long timestamp = TimeGen();
		while (timestamp <= lastTimestamp)
		{
			timestamp = TimeGen();
		}

		return timestamp;
	}

	private long TimeGen()
	{
		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}