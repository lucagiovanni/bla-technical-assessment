namespace TaskManager.Infrastructure.Persistence;

public sealed class TaskRecord
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly DueDate { get; set; }
}
