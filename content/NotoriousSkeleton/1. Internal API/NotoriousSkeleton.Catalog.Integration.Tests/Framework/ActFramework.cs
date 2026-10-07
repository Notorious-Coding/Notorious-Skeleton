using System.Net.Http.Formatting;
using System.Net.Http.Json;
using NotoriousSkeleton.Catalog.Application.Commands.AddProduct;
using NotoriousSkeleton.Catalog.Application.Queries.GetAllProducts.ReadModels;
using NotoriousSkeleton.Catalog.Integration.Tests.Environment;
using NotoriousSkeleton.Libraries.Result;

using NotoriousTest.Web;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Framework;

internal class ActFramework
{
    private static readonly IList<MediaTypeFormatter> Formatters = new List<MediaTypeFormatter> { new ValueObjectAwareJsonFormatter() };

    private readonly NotoriousSkeletonCatalogEnvironment _environment;

    public ActFramework(NotoriousSkeletonCatalogEnvironment environment)
    {
        _environment = environment;
    }

    private HttpClient HttpClient => _environment.GetWebApplication().HttpClient!;

    public async Task<IEnumerable<ProductReadModel>> GetAllProducts()
    {
        HttpResponseMessage? response = await HttpClient.GetAsync("/api/v1.0/product");

        response.EnsureSuccessStatusCode();
        IEnumerable<ProductReadModel> productReadModel = await response.Content.ReadAsAsync<List<ProductReadModel>>(Formatters);

        return productReadModel;
    }

    public async Task<Result<ProductReadModel>> Create(AddProductCommandParameters @params)
    {
        HttpResponseMessage? response = await HttpClient.PostAsJsonAsync("/api/v1.0/product", @params);

        response.EnsureSuccessStatusCode();
        Result<ProductReadModel> productReadModel = await response.Content.ReadAsAsync<Result<ProductReadModel>>(Formatters);

        return productReadModel;
    }
}
