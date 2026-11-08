using TaskManager.Domain;

namespace TaskManager.Application.Tasks;

public interface ITaskRepository
{
    Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskItem>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);

    Task RemoveAsync(TaskItem task, CancellationToken cancellationToken);
}
