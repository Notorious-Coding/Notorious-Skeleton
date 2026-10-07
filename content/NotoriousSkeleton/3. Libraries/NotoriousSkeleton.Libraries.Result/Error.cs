namespace NotoriousSkeleton.Libraries.Result;

public sealed record Error(string Code, string Description, string? Field = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}