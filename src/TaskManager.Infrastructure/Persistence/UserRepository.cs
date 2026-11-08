using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Users;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        return record is null ? null : ToDomain(record);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var record = await _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        return record is null ? null : ToDomain(record);
    }

    public async Task<bool> AddAsync(User user, CancellationToken cancellationToken)
    {
        var record = new UserRecord
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash
        };
        _db.Users.Add(record);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsDuplicate(exception))
        {
            _db.Entry(record).State = EntityState.Detached;
            return false;
        }
    }

    private static User ToDomain(UserRecord record) => new(record.Id, record.Email, record.PasswordHash);

    private static bool IsDuplicate(DbUpdateException exception)
    {
        if (exception.InnerException is not SqliteException sqlite || sqlite.SqliteErrorCode != 19)
        {
            return false;
        }

        return sqlite.SqliteExtendedErrorCode is 2067 or 1555
            || sqlite.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
    }
}
