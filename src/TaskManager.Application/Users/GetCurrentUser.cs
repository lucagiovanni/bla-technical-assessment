namespace TaskManager.Application.Users;

public sealed class GetCurrentUser
{
    public const string UnauthenticatedMessage = "Authentication is required.";

    private readonly IUserRepository _users;

    public GetCurrentUser(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Outcome<UserModel>> ExecuteAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Outcome<UserModel>.Fail(Failure.Unauthorized(UnauthenticatedMessage));
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Outcome<UserModel>.Fail(Failure.Unauthorized(UnauthenticatedMessage));
        }

        return Outcome<UserModel>.Success(new UserModel(user.Id, user.Email));
    }
}
