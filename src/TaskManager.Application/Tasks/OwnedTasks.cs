using TaskManager.Domain;

namespace TaskManager.Application.Tasks;

internal static class OwnedTasks
{
    public const string NotFoundMessage = "Task was not found.";

    public static async Task<Outcome<TaskItem>> FindAsync(
        ITaskRepository tasks,
        Guid ownerId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        if (ownerId == Guid.Empty)
        {
            return Outcome<TaskItem>.Fail(Failure.Validation("Owner is required."));
        }

        var task = await tasks.FindByIdAsync(taskId, cancellationToken);
        if (task is null || task.OwnerId != ownerId)
        {
            return Outcome<TaskItem>.Fail(Failure.NotFound(NotFoundMessage));
        }

        return Outcome<TaskItem>.Success(task);
    }
}
