namespace sevencat.ruoyi.common.util;

public static class NullableExt
{
	public static T ValueOrDefault<T>(this T? na, T defaultValue = default) where T : struct
	{
		return na ?? defaultValue;
	}
}