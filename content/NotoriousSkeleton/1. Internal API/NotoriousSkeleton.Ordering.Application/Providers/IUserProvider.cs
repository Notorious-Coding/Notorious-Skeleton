using NotoriousSkeleton.Ordering.Application.Authentication;

namespace NotoriousSkeleton.Ordering.Application.Providers;

public interface IUserProvider
{
    public User? User { get; }
    public bool IsAuthenticated { get; }
}
