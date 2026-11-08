using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Tasks;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Persistence;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<TaskItem>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var records = await _db.Tasks.AsNoTracking()
            .Where(task => task.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
        return records.Select(ToDomain).ToArray();
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        _db.Tasks.Add(ToRecord(task));
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var record = await _db.Tasks.FirstOrDefaultAsync(item => item.Id == task.Id, cancellationToken);
        if (record is null)
        {
            throw new InvalidOperationException("Task was not found.");
        }

        record.OwnerId = task.OwnerId;
        record.Title = task.Title;
        record.Description = task.Description;
        record.Status = task.Status.ToString();
        record.DueDate = task.DueDate;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var record = await _db.Tasks.FirstOrDefaultAsync(item => item.Id == task.Id, cancellationToken);
        if (record is null)
        {
            return;
        }

        _db.Tasks.Remove(record);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static TaskRecord ToRecord(TaskItem task) => new()
    {
        Id = task.Id,
        OwnerId = task.OwnerId,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status.ToString(),
        DueDate = task.DueDate
    };

    private static TaskItem ToDomain(TaskRecord record) => new(
        record.Id,
        record.OwnerId,
        record.Title,
        record.Description,
        Enum.Parse<TaskState>(record.Status),
        record.DueDate);
}
