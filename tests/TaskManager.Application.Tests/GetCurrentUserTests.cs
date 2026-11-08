using TaskManager.Application.Users;
using TaskManager.Domain;

namespace TaskManager.Application.Tests;

public class GetCurrentUserTests
{
    private readonly UserRepositoryFake _users = new();
    private readonly GetCurrentUser _current;

    public GetCurrentUserTests()
    {
        _current = new GetCurrentUser(_users);
    }

    [Fact]
    public async Task Returns_the_stored_user()
    {
        var user = new User(Guid.NewGuid(), "ada@example.com", "hash");
        Assert.True(await _users.AddAsync(user, CancellationToken.None));

        var result = await _current.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Email, result.Value.Email);
    }

    [Fact]
    public async Task Rejects_a_missing_user_and_an_empty_id()
    {
        var missing = await _current.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
        var empty = await _current.ExecuteAsync(Guid.Empty, CancellationToken.None);

        Assert.Equal(FailureKind.Unauthorized, missing.Error?.Kind);
        Assert.Equal(GetCurrentUser.UnauthenticatedMessage, missing.Error?.Message);
        Assert.Equal(missing.Error?.Message, empty.Error?.Message);
    }
}
