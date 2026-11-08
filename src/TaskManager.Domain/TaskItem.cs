namespace TaskManager.Domain;

public sealed class TaskItem
{
    public const int MaxTitleLength = 200;

    public const int MaxDescriptionLength = 4000;

    public Guid Id { get; }

    public Guid OwnerId { get; }

    public string Title { get; }

    public string Description { get; }

    public TaskState Status { get; }

    public DateOnly DueDate { get; }

    public TaskItem(Guid id, Guid ownerId, string title, string description, TaskState status, DateOnly dueDate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Task id is required.", nameof(id));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id is required.", nameof(ownerId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title must be {MaxTitleLength} characters or fewer.", nameof(title));
        }

        if (description is null)
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Description must be {MaxDescriptionLength} characters or fewer.", nameof(description));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException("Status is not valid.", nameof(status));
        }

        Id = id;
        OwnerId = ownerId;
        Title = trimmedTitle;
        Description = trimmedDescription;
        Status = status;
        DueDate = dueDate;
    }
}
