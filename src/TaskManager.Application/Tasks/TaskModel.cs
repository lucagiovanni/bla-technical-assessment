namespace TaskManager.Application.Tasks;

public sealed record TaskModel(Guid Id, string Title, string Description, string Status, DateOnly DueDate);
