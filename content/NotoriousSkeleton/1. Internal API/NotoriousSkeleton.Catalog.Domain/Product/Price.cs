using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;
using NotoriousSkeleton.Libraries.Domain.Tools.ValueObjects;

namespace NotoriousSkeleton.Catalog.Domain.Product;

public enum PriceError { MustBePositive }

public record struct Price : IValueObject<decimal>
{
    public decimal Value { get; }

    public Price(decimal price)
    {
        if (price < 0)
        {
            throw new NegativeValueDomainException<PriceError>(PriceError.MustBePositive, nameof(price));
        }

        Value = price;
    }

    #region Operators
    public static implicit operator Price(decimal price) => new Price(price);
    public static implicit operator decimal(Price price) => price.Value;
    #endregion
}
