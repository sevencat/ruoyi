using System.Text.Json;
using System.Text.Json.Serialization;

namespace sevencat.ruoyi.config.impl;

public class LongToStringConverter : JsonConverter<long>
{
	// 序列化：将 long 写入为字符串（带双引号）
	public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

	// 反序列化：读取字符串或数字并转回 long
	public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			if (long.TryParse(reader.GetString(), out long result))
			{
				return result;
			}
		}
		else if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt64();
		}
        
		throw new JsonException($"无法将 {reader.TokenType} 转换为 long。");
	}
}