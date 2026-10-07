namespace NotoriousSkeleton.Libraries.Domain.Tools.ValueObjects;

public interface IValueObject<T> where T : notnull
{
    T Value { get; }
}