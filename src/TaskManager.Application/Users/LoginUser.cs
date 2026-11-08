namespace TaskManager.Application.Users;

public sealed class LoginUser
{
    public const string InvalidCredentialsMessage = "Invalid email or password.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwords;

    public LoginUser(IUserRepository users, IPasswordHasher passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<Outcome<UserModel>> ExecuteAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Outcome<UserModel>.Fail(Failure.Validation("Email is required."));
        }

        if (string.IsNullOrEmpty(password))
        {
            return Outcome<UserModel>.Fail(Failure.Validation("Password is required."));
        }

        var normalized = email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(normalized, cancellationToken);
        if (user is null || !_passwords.Verify(password, user.PasswordHash))
        {
            return Outcome<UserModel>.Fail(Failure.Unauthorized(InvalidCredentialsMessage));
        }

        return Outcome<UserModel>.Success(new UserModel(user.Id, user.Email));
    }
}
