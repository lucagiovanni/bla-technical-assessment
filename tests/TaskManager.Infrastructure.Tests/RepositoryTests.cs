using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Tests;

public class RepositoryTests
{
    [Fact]
    public async Task Users_can_be_created_and_read_and_duplicate_emails_are_rejected()
    {
        using var database = new SqliteDatabase();
        var user = new User(Guid.NewGuid(), "ada@example.com", "hash");

        await using (var write = database.CreateContext())
        {
            var repository = new UserRepository(write);
            Assert.True(await repository.AddAsync(user, CancellationToken.None));
            Assert.False(await repository.AddAsync(new User(Guid.NewGuid(), user.Email, "other-hash"), CancellationToken.None));
        }

        await using var read = database.CreateContext();
        var stored = new UserRepository(read);
        var byEmail = await stored.FindByEmailAsync(user.Email, CancellationToken.None);
        var byId = await stored.FindByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(byEmail);
        Assert.NotNull(byId);
        Assert.Equal(user.Id, byEmail.Id);
        Assert.Equal(user.Email, byEmail.Email);
        Assert.Equal(user.PasswordHash, byEmail.PasswordHash);
        Assert.Equal(byEmail.Id, byId.Id);
        Assert.Null(await stored.FindByEmailAsync("missing@example.com", CancellationToken.None));
    }

    [Fact]
    public async Task Tasks_can_be_created_read_updated_and_deleted()
    {
        using var database = new SqliteDatabase();
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var task = new TaskItem(
            Guid.NewGuid(),
            ownerId,
            "Write the brief",
            "Demo notes",
            TaskState.Pending,
            new DateOnly(2026, 11, 9));

        await using (var write = database.CreateContext())
        {
            var users = new UserRepository(write);
            Assert.True(await users.AddAsync(new User(ownerId, "ada@example.com", "hash"), CancellationToken.None));
            Assert.True(await users.AddAsync(new User(otherOwnerId, "other@example.com", "hash"), CancellationToken.None));
            await new TaskRepository(write).AddAsync(task, CancellationToken.None);
        }

        await using (var read = database.CreateContext())
        {
            var repository = new TaskRepository(read);
            var stored = await repository.FindByIdAsync(task.Id, CancellationToken.None);
            Assert.NotNull(stored);
            Assert.Equal(task.OwnerId, stored.OwnerId);
            Assert.Equal(task.Title, stored.Title);
            Assert.Equal(task.Description, stored.Description);
            Assert.Equal(task.Status, stored.Status);
            Assert.Equal(task.DueDate, stored.DueDate);
            Assert.Equal(task.Id, Assert.Single(await repository.ListByOwnerAsync(ownerId, CancellationToken.None)).Id);
            Assert.Empty(await repository.ListByOwnerAsync(otherOwnerId, CancellationToken.None));
        }

        var updated = new TaskItem(task.Id, ownerId, "Write the notes", "Updated", TaskState.Completed, new DateOnly(2026, 12, 1));
        await using (var change = database.CreateContext())
        {
            await new TaskRepository(change).UpdateAsync(updated, CancellationToken.None);
        }

        await using (var readUpdated = database.CreateContext())
        {
            var repository = new TaskRepository(readUpdated);
            var stored = await repository.FindByIdAsync(task.Id, CancellationToken.None);
            Assert.NotNull(stored);
            Assert.Equal("Write the notes", stored.Title);
            Assert.Equal("Updated", stored.Description);
            Assert.Equal(TaskState.Completed, stored.Status);
            Assert.Equal(new DateOnly(2026, 12, 1), stored.DueDate);
            Assert.Equal(ownerId, stored.OwnerId);
            await repository.RemoveAsync(stored, CancellationToken.None);
        }

        await using var afterDelete = database.CreateContext();
        var remaining = new TaskRepository(afterDelete);
        Assert.Null(await remaining.FindByIdAsync(task.Id, CancellationToken.None));
        Assert.Empty(await remaining.ListByOwnerAsync(ownerId, CancellationToken.None));
    }

    [Fact]
    public async Task A_task_requires_a_stored_user()
    {
        using var database = new SqliteDatabase();
        await using var context = database.CreateContext();
        var repository = new TaskRepository(context);
        var task = new TaskItem(Guid.NewGuid(), Guid.NewGuid(), "Orphan", "No owner row", TaskState.Pending, new DateOnly(2026, 11, 9));

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(task, CancellationToken.None));
    }
}
