using TaskManager.Domain;

namespace TaskManager.Application.Tasks;

public sealed class CreateTask
{
    private readonly ITaskRepository _tasks;

    public CreateTask(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<Outcome<TaskModel>> ExecuteAsync(
        Guid ownerId,
        string? title,
        string? description,
        string? status,
        DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        if (ownerId == Guid.Empty)
        {
            return Outcome<TaskModel>.Fail(Failure.Validation("Owner is required."));
        }

        var parsed = TaskRules.Parse(title, description, status, dueDate);
        if (parsed.Error is not null || parsed.Value is null)
        {
            return Outcome<TaskModel>.Fail(parsed.Error ?? Failure.Validation("Task is not valid."));
        }

        var task = new TaskItem(
            Guid.NewGuid(),
            ownerId,
            parsed.Value.Title,
            parsed.Value.Description,
            parsed.Value.Status,
            parsed.Value.DueDate);

        await _tasks.AddAsync(task, cancellationToken);
        return Outcome<TaskModel>.Success(TaskRules.ToModel(task));
    }
}
