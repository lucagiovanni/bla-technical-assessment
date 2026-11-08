using TaskManager.Domain;

namespace TaskManager.Application.Tasks;

public sealed class UpdateTask
{
    private readonly ITaskRepository _tasks;

    public UpdateTask(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<Outcome<TaskModel>> ExecuteAsync(
        Guid ownerId,
        Guid taskId,
        string? title,
        string? description,
        string? status,
        DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        var owned = await OwnedTasks.FindAsync(_tasks, ownerId, taskId, cancellationToken);
        if (owned.Error is not null || owned.Value is null)
        {
            return Outcome<TaskModel>.Fail(owned.Error ?? Failure.NotFound(OwnedTasks.NotFoundMessage));
        }

        var parsed = TaskRules.Parse(title, description, status, dueDate);
        if (parsed.Error is not null || parsed.Value is null)
        {
            return Outcome<TaskModel>.Fail(parsed.Error ?? Failure.Validation("Task is not valid."));
        }

        var updated = new TaskItem(
            owned.Value.Id,
            owned.Value.OwnerId,
            parsed.Value.Title,
            parsed.Value.Description,
            parsed.Value.Status,
            parsed.Value.DueDate);

        await _tasks.UpdateAsync(updated, cancellationToken);
        return Outcome<TaskModel>.Success(TaskRules.ToModel(updated));
    }
}
