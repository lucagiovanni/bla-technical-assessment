using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Tests;

internal sealed class SqliteDatabase : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"taskmanager-{Guid.NewGuid():N}.db");
    private readonly List<AppDbContext> _contexts = [];

    public string ConnectionString => $"Data Source={_path}";

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(ConnectionString).Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        _contexts.Add(context);
        return context;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            var candidate = _path + suffix;
            if (File.Exists(candidate))
            {
                File.Delete(candidate);
            }
        }
    }
}
