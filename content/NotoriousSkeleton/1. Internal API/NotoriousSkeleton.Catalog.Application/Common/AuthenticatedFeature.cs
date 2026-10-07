using NotoriousSkeleton.Catalog.Application.Authentication;
using NotoriousSkeleton.Catalog.Application.Providers;

using System;

namespace NotoriousSkeleton.Catalog.Application.Common;

public class AuthenticatedFeature
{
    protected readonly IUserProvider UserProvider;
    protected User User => UserProvider.User ?? throw new UnauthorizedAccessException();

    public AuthenticatedFeature(IUserProvider userProvider)
    {
        UserProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
    }

    protected void EnsureAuthenticated()
    {
        if (!UserProvider.IsAuthenticated || UserProvider.User == null)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
