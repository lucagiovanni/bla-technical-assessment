using TaskManager.Application.Users;

namespace TaskManager.Application.Tests;

public class UserUseCaseTests
{
    private const string Password = "Demo123!";
    private readonly UserRepositoryFake _users = new();
    private readonly PasswordHasherFake _passwords = new();
    private readonly RegisterUser _register;
    private readonly LoginUser _login;

    public UserUseCaseTests()
    {
        _register = new RegisterUser(_users, _passwords);
        _login = new LoginUser(_users, _passwords);
    }

    [Fact]
    public async Task Register_stores_a_normalized_email_and_a_password_hash()
    {
        var registered = await _register.ExecuteAsync("  Ada@Example.com  ", Password, CancellationToken.None);

        var model = AssertSuccess(registered);
        Assert.Equal("ada@example.com", model.Email);
        Assert.NotEqual(Guid.Empty, model.Id);

        var stored = Assert.Single(_users.Users);
        Assert.Equal(model.Id, stored.Id);
        Assert.Equal("ada@example.com", stored.Email);
        Assert.Equal("hash:" + Password, stored.PasswordHash);
        Assert.NotEqual(Password, stored.PasswordHash);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email()
    {
        AssertSuccess(await _register.ExecuteAsync("ada@example.com", Password, CancellationToken.None));

        var duplicate = await _register.ExecuteAsync("Ada@Example.com", "Other123", CancellationToken.None);

        AssertFailure(duplicate, FailureKind.Conflict, RegisterUser.DuplicateMessage);
        Assert.Single(_users.Users);
        Assert.Equal("hash:" + Password, _users.Users[0].PasswordHash);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_discovered_while_saving()
    {
        _users.RejectNextAdd = true;

        var registered = await _register.ExecuteAsync("ada@example.com", Password, CancellationToken.None);

        AssertFailure(registered, FailureKind.Conflict, RegisterUser.DuplicateMessage);
        Assert.Empty(_users.Users);
    }

    [Theory]
    [InlineData(null, Password, "Email is required.")]
    [InlineData("   ", Password, "Email is required.")]
    [InlineData("not-an-email", Password, "Email is not valid.")]
    [InlineData("ada@example", Password, "Email is not valid.")]
    [InlineData("ada@example.com", null, "Password is required.")]
    [InlineData("ada@example.com", "", "Password is required.")]
    [InlineData("ada@example.com", "short1", "Password must be at least 8 characters and include a letter and a digit.")]
    [InlineData("ada@example.com", "longpassword", "Password must be at least 8 characters and include a letter and a digit.")]
    [InlineData("ada@example.com", "12345678", "Password must be at least 8 characters and include a letter and a digit.")]
    public async Task Register_rejects_invalid_email_or_password(string? email, string? password, string message)
    {
        var registered = await _register.ExecuteAsync(email, password, CancellationToken.None);

        AssertFailure(registered, FailureKind.Validation, message);
        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task Register_rejects_an_email_that_is_too_long()
    {
        var email = new string('a', 309) + "@example.com";

        var registered = await _register.ExecuteAsync(email, Password, CancellationToken.None);

        AssertFailure(registered, FailureKind.Validation, "Email is not valid.");
        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task Register_rejects_a_password_that_is_too_long()
    {
        var password = "a1" + new string('b', 199);

        var registered = await _register.ExecuteAsync("ada@example.com", password, CancellationToken.None);

        AssertFailure(registered, FailureKind.Validation, "Password must be 200 characters or fewer.");
        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task Login_accepts_the_registered_password_and_rejects_bad_credentials()
    {
        var registered = AssertSuccess(await _register.ExecuteAsync("Ada@Example.com", Password, CancellationToken.None));

        var signedIn = AssertSuccess(await _login.ExecuteAsync("  ada@example.com ", Password, CancellationToken.None));
        Assert.Equal(registered.Id, signedIn.Id);
        Assert.Equal(registered.Email, signedIn.Email);

        var wrongPassword = await _login.ExecuteAsync("ada@example.com", "Wrong123", CancellationToken.None);
        var unknownEmail = await _login.ExecuteAsync("other@example.com", Password, CancellationToken.None);

        AssertFailure(wrongPassword, FailureKind.Unauthorized, LoginUser.InvalidCredentialsMessage);
        AssertFailure(unknownEmail, FailureKind.Unauthorized, LoginUser.InvalidCredentialsMessage);
        Assert.Equal(wrongPassword.Error!.Message, unknownEmail.Error!.Message);
    }

    [Theory]
    [InlineData(null, Password, "Email is required.")]
    [InlineData("  ", Password, "Email is required.")]
    [InlineData("ada@example.com", null, "Password is required.")]
    [InlineData("ada@example.com", "", "Password is required.")]
    public async Task Login_requires_an_email_and_password(string? email, string? password, string message)
    {
        AssertSuccess(await _register.ExecuteAsync("ada@example.com", Password, CancellationToken.None));

        var signedIn = await _login.ExecuteAsync(email, password, CancellationToken.None);

        AssertFailure(signedIn, FailureKind.Validation, message);
    }

    private static UserModel AssertSuccess(Outcome<UserModel> outcome)
    {
        Assert.True(outcome.IsSuccess);
        Assert.NotNull(outcome.Value);
        return outcome.Value;
    }

    private static void AssertFailure(Outcome<UserModel> outcome, FailureKind kind, string message)
    {
        Assert.False(outcome.IsSuccess);
        Assert.NotNull(outcome.Error);
        Assert.Equal(kind, outcome.Error.Kind);
        Assert.Equal(message, outcome.Error.Message);
    }
}
