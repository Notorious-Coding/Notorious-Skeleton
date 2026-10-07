using NotoriousSkeleton.Libraries.Authentication;
using NotoriousSkeleton.Libraries.Authentication.Configuration;
using NotoriousSkeleton.Libraries.Configuration;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace NotoriousSkeleton.API.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddAuthentication((options) =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtAuthentication(configuration.BindSection<ClientCredentialAuthenticationOptions>());

        return services;
    }
}
