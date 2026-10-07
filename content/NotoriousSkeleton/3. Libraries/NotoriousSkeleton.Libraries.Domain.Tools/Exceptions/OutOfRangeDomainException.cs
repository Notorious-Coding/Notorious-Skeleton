using System;
using System.Numerics;

namespace NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;

public class OutOfRangeDomainException<TError, TValue> : DomainException<TError>
    where TError : Enum
    where TValue : INumber<TValue>
{
    public OutOfRangeDomainException(TError error, string param, TValue min, TValue max)
        : base(error, $"{param} must remain between {min} and {max}")
    {
    }

    public OutOfRangeDomainException(TError error, string param, TValue min, TValue max, Exception? innerException)
        : base(error, $"{param} must remain between {min} and {max}", innerException)
    {
    }
}
