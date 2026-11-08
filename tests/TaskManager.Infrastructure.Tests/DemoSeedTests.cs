using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure.Tests;

public class DemoSeedTests
{
    [Fact]
    public async Task Stores_the_demo_user_and_sample_tasks_once()
    {
        using var database = new SqliteDatabase();
        var passwords = new Pbkdf2PasswordHasher();

        await using (var seed = database.CreateContext())
        {
            await DemoSeed.ApplyAsync(seed, passwords, CancellationToken.None);
            await DemoSeed.ApplyAsync(seed, passwords, CancellationToken.None);
        }

        await using var read = database.CreateContext();
        await DemoSeed.ApplyAsync(read, passwords, CancellationToken.None);

        var users = new UserRepository(read);
        var tasks = new TaskRepository(read);
        var user = await users.FindByEmailAsync(DemoSeed.Email, CancellationToken.None);
        var byId = await users.FindByIdAsync(DemoSeed.UserId, CancellationToken.None);
        var storedTasks = await tasks.ListByOwnerAsync(DemoSeed.UserId, CancellationToken.None);

        Assert.NotNull(user);
        Assert.NotNull(byId);
        Assert.Equal(user.Id, byId.Id);
        Assert.Equal(DemoSeed.UserId, user.Id);
        Assert.Equal(DemoSeed.Email, user.Email);
        Assert.NotEqual(DemoSeed.Password, user.PasswordHash);
        Assert.True(passwords.Verify(DemoSeed.Password, user.PasswordHash));
        Assert.False(passwords.Verify("wrong-password", user.PasswordHash));
        Assert.Equal(1, await read.Users.CountAsync());
        Assert.Equal(3, await read.Tasks.CountAsync());
        Assert.Equal(3, storedTasks.Count);
        Assert.Contains(storedTasks, task =>
            task.Id == DemoSeed.ReviewTaskId &&
            task.OwnerId == DemoSeed.UserId &&
            task.Title == "Review the project brief" &&
            task.Description.Length > 0 &&
            task.Status == TaskState.Pending &&
            task.DueDate == new DateOnly(2026, 11, 9));
        Assert.Contains(storedTasks, task =>
            task.Id == DemoSeed.SketchTaskId &&
            task.OwnerId == DemoSeed.UserId &&
            task.Title == "Sketch the task board" &&
            task.Status == TaskState.InProgress &&
            task.DueDate == new DateOnly(2026, 11, 10));
        Assert.Contains(storedTasks, task =>
            task.Id == DemoSeed.PrepareTaskId &&
            task.OwnerId == DemoSeed.UserId &&
            task.Title == "Prepare the demo" &&
            task.Status == TaskState.Completed &&
            task.DueDate == new DateOnly(2026, 11, 8));
    }
}
