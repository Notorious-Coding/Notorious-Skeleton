using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Web.Applications;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Infrastructure;

public class InternalAPIApplication : WebApplication<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
                options.DefaultScheme = TestAuthHandler.Scheme
            ).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        });

    }
}
