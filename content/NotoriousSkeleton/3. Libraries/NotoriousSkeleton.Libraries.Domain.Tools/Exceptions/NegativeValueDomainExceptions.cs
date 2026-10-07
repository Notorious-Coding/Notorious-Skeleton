using System;

namespace NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;

public class NegativeValueDomainException<TError> : DomainException<TError> where TError : Enum
{
    public NegativeValueDomainException(TError error, string param) : base(error, $"{param} must be greater than zero.")
    {
    }
}
