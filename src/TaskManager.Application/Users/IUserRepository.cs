using TaskManager.Domain;

namespace TaskManager.Application.Users;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the user. Returns false when the email is already present.
    /// </summary>
    Task<bool> AddAsync(User user, CancellationToken cancellationToken);
}
