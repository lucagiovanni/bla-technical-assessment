using TaskManager.Application.Tasks;
using TaskManager.Domain;

namespace TaskManager.Application.Tests;

internal sealed class TaskRepositoryFake : ITaskRepository
{
    public List<TaskItem> Items { get; } = [];

    public bool ReturnEveryOwner { get; set; }

    public int AddCount { get; private set; }

    public int UpdateCount { get; private set; }

    public int RemoveCount { get; private set; }

    public Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.FirstOrDefault(task => task.Id == id));
    }

    public Task<IReadOnlyList<TaskItem>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        IEnumerable<TaskItem> items = ReturnEveryOwner
            ? Items
            : Items.Where(task => task.OwnerId == ownerId);
        return Task.FromResult<IReadOnlyList<TaskItem>>(items.ToList());
    }

    public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        AddCount++;
        Items.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        UpdateCount++;
        var index = Items.FindIndex(item => item.Id == task.Id);
        if (index >= 0)
        {
            Items[index] = task;
        }
        else
        {
            Items.Add(task);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(TaskItem task, CancellationToken cancellationToken)
    {
        RemoveCount++;
        Items.RemoveAll(item => item.Id == task.Id);
        return Task.CompletedTask;
    }
}
