using Asp.Versioning;

using NotoriousSkeleton.API.Authentication;
using NotoriousSkeleton.Catalog;
using NotoriousSkeleton.Ordering;

using Serilog;

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using NotoriousSkeleton.API.Handlers;
using NotoriousSkeleton.API.Serializer;
using NotoriousSkeleton.Ordering.Client.Catalog.Extensions;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
using var log = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Logger = log;

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddOrderingCatalogClients();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi((options) =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = null;
            schema.Enum = Enum.GetNames(context.JsonTypeInfo.Type)
                .Select(n => (JsonNode)JsonValue.Create(n))
                .ToList();
        }
        return Task.CompletedTask;
    });
});
builder.Services.AddControllers()
    .AddCatalogControllers()
    .AddOrderingControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonValueObjectConverterFactory());
    });

builder.Services.AddApplicationAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
}).AddMvc();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public",
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            })
    );
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Host.UseSerilog();
try
{

    Log.Information("NotoriousSkeleton API starting");

    string? version = builder.Configuration["Build:Version"];
    string? commitSha = builder.Configuration["Build:CommitSha"];
    string? branch = builder.Configuration["Build:Branch"];

    Log.Information("Running version {Version} from commit {CommitSha} on branch {Branch}", version, commitSha, branch);
    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandler();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();
    app.MapFallbackToFile("/index.html");

    app.Run();
    Log.Information("NotoriousSkeleton API successfully started");

}
catch (Exception ex)
{
    Log.Fatal(ex, "NotoriousSkeleton API failed to start");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program
{

}
