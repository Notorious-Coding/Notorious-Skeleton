using NotoriousSkeleton.Libraries.Domain.Tools.Exceptions;
using NotoriousSkeleton.SharedKernel.User;

namespace NotoriousSkeleton.SharedKernel.UnitTests.User;

public class EmailTests
{
    [Fact]
    public void Email_Should_Be_Created_When_Value_Is_Valid()
    {
        var email = new Email("john.doe@notoriousskeleton.io");

        Assert.Equal("john.doe@notoriousskeleton.io", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_Should_Throw_When_Value_Is_Empty(string value)
    {
        var exception = Assert.Throws<DomainException<EmailError>>(() => new Email(value));
        Assert.Equal(EmailError.Required, exception.Error);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("john.doe@")]
    [InlineData("@notoriousskeleton.io")]
    public void Email_Should_Throw_When_Value_Is_Invalid(string value)
    {
        var exception = Assert.Throws<DomainException<EmailError>>(() => new Email(value));
        Assert.Equal(EmailError.Invalid, exception.Error);
    }

    [Fact]
    public void TryCreate_Should_Return_True_And_Email_When_Value_Is_Valid()
    {
        var created = Email.TryCreate("john.doe@notoriousskeleton.io", out var email);

        Assert.True(created);
        Assert.Equal("john.doe@notoriousskeleton.io", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void TryCreate_Should_Return_False_When_Value_Is_Invalid(string? value)
    {
        var created = Email.TryCreate(value, out var email);

        Assert.False(created);
        Assert.Equal(default, email);
    }

    [Fact]
    public void Email_Should_Convert_To_Its_String_Value()
    {
        var email = new Email("john.doe@notoriousskeleton.io");

        string value = email;

        Assert.Equal("john.doe@notoriousskeleton.io", value);
        Assert.Equal("john.doe@notoriousskeleton.io", email.ToString());
    }
}
