namespace NotoriousSkeleton.Libraries.Enumeration;

public static class EnumExtensions
{
    public static TEnum? ToEnumOrNull<TEnum>(this string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : null;
    }
}