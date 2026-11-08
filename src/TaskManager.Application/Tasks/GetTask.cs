namespace TaskManager.Application.Tasks;

public sealed class GetTask
{
    private readonly ITaskRepository _tasks;

    public GetTask(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<Outcome<TaskModel>> ExecuteAsync(Guid ownerId, Guid taskId, CancellationToken cancellationToken)
    {
        var owned = await OwnedTasks.FindAsync(_tasks, ownerId, taskId, cancellationToken);
        if (owned.Error is not null || owned.Value is null)
        {
            return Outcome<TaskModel>.Fail(owned.Error ?? Failure.NotFound(OwnedTasks.NotFoundMessage));
        }

        return Outcome<TaskModel>.Success(TaskRules.ToModel(owned.Value));
    }
}
