using NotoriousSkeleton.Ordering.Application.Authentication;
using NotoriousSkeleton.Ordering.Application.Providers;

using System;

namespace NotoriousSkeleton.Ordering.Application.Common;

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
