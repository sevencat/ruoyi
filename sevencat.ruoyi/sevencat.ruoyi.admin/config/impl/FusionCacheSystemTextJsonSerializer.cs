using System.Text.Json;
using ZiggyCreatures.Caching.Fusion.Serialization;

namespace sevencat.ruoyi.config.impl;

public class FusionCacheSystemTextJsonSerializer
	: IFusionCacheSerializer
{
	/// <summary>
	/// The options class for the <see cref="FusionCacheSystemTextJsonSerializer"/> class.
	/// </summary>
	public class Options
	{
		/// <summary>
		/// The optional <see cref="JsonSerializerOptions"/> object to use.
		/// </summary>
		public JsonSerializerOptions SerializerOptions { get; set; }
	}

	/// <summary>
	/// Creates a new instance of a <see cref="FusionCacheSystemTextJsonSerializer"/> object.
	/// </summary>
	/// <param name="options">The optional <see cref="JsonSerializerOptions"/> object to use.</param>
	public FusionCacheSystemTextJsonSerializer(JsonSerializerOptions options = null)
		: this(new Options { SerializerOptions = options })
	{
		// EMPTY
	}

	/// <summary>
	/// Creates a new instance of a <see cref="FusionCacheSystemTextJsonSerializer"/> object.
	/// </summary>
	/// <param name="options">The optional <see cref="Options"/> object to use.</param>
	public FusionCacheSystemTextJsonSerializer(Options options)
	{
		_options = options;
	}

	private readonly Options _options;

	/// <inheritdoc />
	public byte[] Serialize<T>(T obj)
	{
		return JsonSerializer.SerializeToUtf8Bytes(obj, _options?.SerializerOptions);
	}

	/// <inheritdoc />
	public T Deserialize<T>(byte[] data)
	{
		return JsonSerializer.Deserialize<T>(data, _options?.SerializerOptions);
	}

	/// <inheritdoc />
	public ValueTask<byte[]> SerializeAsync<T>(T obj, CancellationToken token = default)
	{
		return new ValueTask<byte[]>(Serialize(obj));
	}

	/// <inheritdoc />
	public ValueTask<T> DeserializeAsync<T>(byte[] data, CancellationToken token = default)
	{
		return new ValueTask<T>(Deserialize<T>(data));
	}

	/// <inheritdoc />
	public override string ToString() => GetType().Name;
}