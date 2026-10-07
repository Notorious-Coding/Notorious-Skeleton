using System.Net.Http.Formatting;
using System.Net.Http.Json;
using NotoriousSkeleton.Libraries.Result;
using NotoriousSkeleton.Ordering.Application.Commands.PlaceOrder;
using NotoriousSkeleton.Ordering.Application.Queries.GetOrderById.ReadModels;
using NotoriousSkeleton.Ordering.Integration.Tests.Environment;

using NotoriousTest.Web;

namespace NotoriousSkeleton.Ordering.Integration.Tests.Framework;

internal class OrderingActFramework
{
    private static readonly IList<MediaTypeFormatter> Formatters = new List<MediaTypeFormatter> { new ValueObjectAwareJsonFormatter() };

    private readonly NotoriousSkeletonOrderingEnvironment _environment;

    public OrderingActFramework(NotoriousSkeletonOrderingEnvironment environment)
    {
        _environment = environment;
    }

    private HttpClient HttpClient => _environment.GetWebApplication().HttpClient!;

    public async Task<Result<OrderReadModel>> PlaceOrder(PlaceOrderCommandParameters @params)
    {
        HttpResponseMessage? response = await HttpClient.PostAsJsonAsync("/api/v1.0/order", @params);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<Result<OrderReadModel>>(Formatters);
    }

    public async Task<Result<OrderReadModel>> GetOrder(Guid id)
    {
        HttpResponseMessage? response = await HttpClient.GetAsync($"/api/v1.0/order/{id}");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<Result<OrderReadModel>>(Formatters);
    }
}
