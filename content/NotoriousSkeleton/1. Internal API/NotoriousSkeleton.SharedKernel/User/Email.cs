using System.Net.Mail;
using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;
using NotoriousSkeleton.Libraries.Domain.Tools.ValueObjects;

namespace NotoriousSkeleton.SharedKernel.User;

public enum EmailError { Required, Invalid }

public record struct Email : IValueObject<string>
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException<EmailError>(EmailError.Required, "Email is required");

        if (!MailAddress.TryCreate(value, out _))
            throw new DomainException<EmailError>(EmailError.Invalid, "Email is invalid");

        Value = value;
    }
    
    public static bool TryCreate(string? value, out Email email)
    {
        email = default;

        if (string.IsNullOrWhiteSpace(value) || !MailAddress.TryCreate(value, out _))
            return false;

        email = new Email(value);
        return true;
    }

    public static implicit operator Email(string value) => new(value);
    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
