using TaskManager.Domain;

namespace TaskManager.Application.Tasks;

internal sealed record ParsedTask(string Title, string Description, TaskState Status, DateOnly DueDate);

internal static class TaskRules
{
    public static Outcome<ParsedTask> Parse(string? title, string? description, string? status, DateOnly? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Outcome<ParsedTask>.Fail(Failure.Validation("Title is required."));
        }

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > TaskItem.MaxTitleLength)
        {
            return Outcome<ParsedTask>.Fail(Failure.Validation($"Title must be {TaskItem.MaxTitleLength} characters or fewer."));
        }

        var trimmedDescription = (description ?? string.Empty).Trim();
        if (trimmedDescription.Length > TaskItem.MaxDescriptionLength)
        {
            return Outcome<ParsedTask>.Fail(Failure.Validation($"Description must be {TaskItem.MaxDescriptionLength} characters or fewer."));
        }

        if (!TryParseStatus(status, out var parsedStatus))
        {
            return Outcome<ParsedTask>.Fail(Failure.Validation("Status is not valid."));
        }

        if (dueDate is null)
        {
            return Outcome<ParsedTask>.Fail(Failure.Validation("Due date is required."));
        }

        return Outcome<ParsedTask>.Success(new ParsedTask(trimmedTitle, trimmedDescription, parsedStatus, dueDate.Value));
    }

    public static TaskModel ToModel(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status.ToString(), task.DueDate);

    private static bool TryParseStatus(string? status, out TaskState parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var match = Enum.GetNames<TaskState>()
            .FirstOrDefault(name => string.Equals(name, status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        parsed = Enum.Parse<TaskState>(match);
        return true;
    }
}
