namespace NotoriousSkeleton.SharedKernel.User;

public record struct UserId(string Value)
{
    public static implicit operator UserId(string id) => new UserId(id);
    public static implicit operator string(UserId id) => id.Value;
}