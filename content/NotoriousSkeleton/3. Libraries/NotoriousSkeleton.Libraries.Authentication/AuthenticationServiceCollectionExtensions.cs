using NotoriousSkeleton.Libraries.Authentication.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace NotoriousSkeleton.Libraries.Authentication;

public static class AuthenticationServiceCollectionExtensions
{

    public static AuthenticationBuilder AddJwtAuthentication(this AuthenticationBuilder auth, ClientCredentialAuthenticationOptions m2mOptions)
    {
        return auth.AddJwtBearer((options) =>
        {
            options.Authority = m2mOptions.Authority;
            options.Audience = m2mOptions.Audience;

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = m2mOptions.Audience,
                ValidateIssuer = true,
                ValidIssuer = m2mOptions.Authority,
            };
            options.MapInboundClaims = false;
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    ILogger log = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Authentication");

                    log.LogError("Authentication failed: {0}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    ILogger log = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Authentication");

                    log.LogInformation("Token validated for {0}", context.Principal.Identity.Name);
                    return Task.CompletedTask;
                }
            };
        });
    }
}
