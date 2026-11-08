using TaskManager.Application.Users;
using TaskManager.Domain;

namespace TaskManager.Application.Tests;

internal sealed class UserRepositoryFake : IUserRepository
{
    private readonly List<User> _users = [];

    public bool RejectNextAdd { get; set; }

    public IReadOnlyList<User> Users => _users;

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.Ordinal)));
    }

    public Task<bool> AddAsync(User user, CancellationToken cancellationToken)
    {
        if (RejectNextAdd || _users.Any(existing => string.Equals(existing.Email, user.Email, StringComparison.Ordinal)))
        {
            RejectNextAdd = false;
            return Task.FromResult(false);
        }

        _users.Add(user);
        return Task.FromResult(true);
    }
}
