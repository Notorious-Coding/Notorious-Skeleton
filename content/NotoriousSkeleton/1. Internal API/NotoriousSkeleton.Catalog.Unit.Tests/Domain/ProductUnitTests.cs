using NotoriousSkeleton.Catalog.Domain.Product;
using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;

namespace NotoriousSkeleton.Catalog.Unit.Tests.Domain;

public class ProductUnitTests
{
    [Fact]
    public void Product_ShouldBeCreated_WhenValuesAreValid()
    {
        var product = new Product("Chair", 49.90m);

        Assert.Equal("Chair", product.Name);
        Assert.Equal(49.90m, product.Price.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_ShouldThrow_WhenNameIsEmpty(string name)
    {
        var exception = Assert.Throws<DomainException<ProductError>>(() => new Product(name, 12));
        Assert.Equal(ProductError.NameRequired, exception.Error);
    }

    [Fact]
    public void Price_ShouldThrow_WhenNegative()
    {
        var exception = Assert.Throws<NegativeValueDomainException<PriceError>>(() => new Price(-1));
        Assert.Equal(PriceError.MustBePositive, exception.Error);
    }
}
