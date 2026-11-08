using TaskManager.Domain;

namespace TaskManager.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Requires_an_identifier()
    {
        var error = Assert.Throws<ArgumentException>(() => new User(Guid.Empty, "ada@example.com", "hash"));

        Assert.Equal("id", error.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Requires_an_email(string? email)
    {
        var error = Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), email!, "hash"));

        Assert.Equal("email", error.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Requires_a_password_hash(string? passwordHash)
    {
        var error = Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), "ada@example.com", passwordHash!));

        Assert.Equal("passwordHash", error.ParamName);
    }

    [Fact]
    public void Stores_the_identifier_email_and_password_hash()
    {
        var id = Guid.NewGuid();

        var user = new User(id, "  ada@example.com  ", "stored-hash");

        Assert.Equal(id, user.Id);
        Assert.Equal("ada@example.com", user.Email);
        Assert.Equal("stored-hash", user.PasswordHash);
    }
}
