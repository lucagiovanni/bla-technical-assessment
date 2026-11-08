using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Users;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Persistence;

public static class DemoSeed
{
    public const string Email = "demo@taskmanager.local";

    public const string Password = "Demo123!";

    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid ReviewTaskId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid SketchTaskId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid PrepareTaskId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static async Task ApplyAsync(AppDbContext db, IPasswordHasher passwords, CancellationToken cancellationToken)
    {
        var alreadySeeded = await db.Users.AnyAsync(
            user => user.Id == UserId || user.Email == Email,
            cancellationToken);
        if (alreadySeeded)
        {
            return;
        }

        db.Users.Add(new UserRecord
        {
            Id = UserId,
            Email = Email,
            PasswordHash = passwords.Hash(Password)
        });

        db.Tasks.AddRange(
            Task(ReviewTaskId, "Review the project brief", "Read the personal task list and note what the demo should show.", TaskState.Pending, new DateOnly(2026, 11, 9)),
            Task(SketchTaskId, "Sketch the task board", "Lay out create, update, and delete for the signed-in person's tasks.", TaskState.InProgress, new DateOnly(2026, 11, 10)),
            Task(PrepareTaskId, "Prepare the demo", "Sign in with the seeded account and walk through one task.", TaskState.Completed, new DateOnly(2026, 11, 8)));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static TaskRecord Task(Guid id, string title, string description, TaskState status, DateOnly dueDate) => new()
    {
        Id = id,
        OwnerId = UserId,
        Title = title,
        Description = description,
        Status = status.ToString(),
        DueDate = dueDate
    };
}
