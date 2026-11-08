namespace TaskManager.Application.Tasks;

public sealed class ListTasks
{
    private readonly ITaskRepository _tasks;

    public ListTasks(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<IReadOnlyList<TaskModel>> ExecuteAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        if (ownerId == Guid.Empty)
        {
            return [];
        }

        var tasks = await _tasks.ListByOwnerAsync(ownerId, cancellationToken);
        return tasks
            .Where(task => task.OwnerId == ownerId)
            .OrderBy(task => task.DueDate)
            .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
            .Select(TaskRules.ToModel)
            .ToArray();
    }
}
