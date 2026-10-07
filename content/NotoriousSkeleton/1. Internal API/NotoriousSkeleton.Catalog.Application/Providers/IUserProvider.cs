using NotoriousSkeleton.Catalog.Application.Authentication;

namespace NotoriousSkeleton.Catalog.Application.Providers;

public interface IUserProvider
{
    public User? User { get; }
    public bool IsAuthenticated { get; }
}
