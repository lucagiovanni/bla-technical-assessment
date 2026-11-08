using TaskManager.Application.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DemoSeed.ApplyAsync(db, passwords, CancellationToken.None);
    }
}
