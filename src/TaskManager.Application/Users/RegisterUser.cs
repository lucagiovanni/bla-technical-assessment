using TaskManager.Domain;

namespace TaskManager.Application.Users;

public sealed class RegisterUser
{
    public const string DuplicateMessage = "An account with that email already exists.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwords;

    public RegisterUser(IUserRepository users, IPasswordHasher passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<Outcome<UserModel>> ExecuteAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        var normalized = UserRules.NormalizeEmail(email);
        if (normalized.Error is not null || normalized.Value is null)
        {
            return Outcome<UserModel>.Fail(normalized.Error ?? Failure.Validation("Email is required."));
        }

        var passwordError = UserRules.ValidatePassword(password);
        if (passwordError is not null)
        {
            return Outcome<UserModel>.Fail(passwordError);
        }

        var existing = await _users.FindByEmailAsync(normalized.Value, cancellationToken);
        if (existing is not null)
        {
            return Outcome<UserModel>.Fail(Failure.Conflict(DuplicateMessage));
        }

        var user = new User(Guid.NewGuid(), normalized.Value, _passwords.Hash(password!));
        var added = await _users.AddAsync(user, cancellationToken);
        if (!added)
        {
            return Outcome<UserModel>.Fail(Failure.Conflict(DuplicateMessage));
        }

        return Outcome<UserModel>.Success(new UserModel(user.Id, user.Email));
    }
}
