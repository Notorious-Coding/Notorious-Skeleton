using NotoriousSkeleton.Catalog.Application.Authentication;
using NotoriousSkeleton.Catalog.Application.Providers;
using NotoriousSkeleton.SharedKernel.User;
using Microsoft.AspNetCore.Http;
namespace NotoriousSkeleton.Catalog.Authentication.Auth0;

internal class Auth0UserProvider : IUserProvider
{
    public User? User { get; init; }
    public bool IsAuthenticated { get; init; } = false;

    public Auth0UserProvider(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;

            string userId = httpContext.User.FindFirst("sub")?.Value ?? throw new ArgumentNullException("sub claim not found");
            User = new User(new UserId(userId));
        }
    }
}
