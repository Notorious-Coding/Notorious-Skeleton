using System;

namespace NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;

/// <summary>
/// Base type for all domain rule violations. Non-generic so callers can <c>catch (DomainException)</c>
/// or <c>Assert.Throws&lt;DomainException&gt;</c> regardless of the concrete error enum.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string? message) : base(message)
    {
    }

    protected DomainException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>Boxed access to the strongly-typed error, for logging or non-generic handlers.</summary>
    public abstract Enum ErrorCode { get; }
}

/// <summary>
/// A domain rule violation carrying a strongly-typed <typeparamref name="TError"/> value describing the cause.
/// The human-readable reason stays in <see cref="Exception.Message"/>.
/// </summary>
public class DomainException<TError> : DomainException where TError : Enum
{
    public TError Error { get; }

    public override Enum ErrorCode => Error;

    public DomainException(TError error, string? message) : base(message)
    {
        Error = error;
    }

    public DomainException(TError error, string? message, Exception? innerException) : base(message, innerException)
    {
        Error = error;
    }
}
