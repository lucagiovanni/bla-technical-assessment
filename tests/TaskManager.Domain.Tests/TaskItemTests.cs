using TaskManager.Domain;

namespace TaskManager.Domain.Tests;

public class TaskItemTests
{
    private static readonly Guid TaskId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OwnerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly DateOnly DueDate = new(2026, 11, 9);

    [Fact]
    public void Requires_an_identifier()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));

        Assert.Equal("id", error.ParamName);
    }

    [Fact]
    public void Requires_an_owner_identifier()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(ownerId: Guid.Empty));

        Assert.Equal("ownerId", error.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Requires_a_title(string? title)
    {
        var error = Assert.Throws<ArgumentException>(() => Create(title: title!));

        Assert.Equal("title", error.ParamName);
    }

    [Fact]
    public void Rejects_a_title_that_is_too_long()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(title: new string('a', TaskItem.MaxTitleLength + 1)));

        Assert.Equal("title", error.ParamName);
    }

    [Fact]
    public void Requires_a_description()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(description: null!));

        Assert.Equal("description", error.ParamName);
    }

    [Fact]
    public void Rejects_a_description_that_is_too_long()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(description: new string('d', TaskItem.MaxDescriptionLength + 1)));

        Assert.Equal("description", error.ParamName);
    }

    [Fact]
    public void Requires_a_valid_status()
    {
        var error = Assert.Throws<ArgumentException>(() => Create(status: (TaskState)42));

        Assert.Equal("status", error.ParamName);
    }

    [Theory]
    [InlineData(TaskState.Pending)]
    [InlineData(TaskState.InProgress)]
    [InlineData(TaskState.Completed)]
    public void Stores_the_task_fields(TaskState status)
    {
        var task = Create(title: "  Write the brief  ", description: "  Notes for the demo  ", status: status);

        Assert.Equal(TaskId, task.Id);
        Assert.Equal(OwnerId, task.OwnerId);
        Assert.Equal("Write the brief", task.Title);
        Assert.Equal("Notes for the demo", task.Description);
        Assert.Equal(status, task.Status);
        Assert.Equal(DueDate, task.DueDate);
    }

    [Fact]
    public void Allows_an_empty_description()
    {
        var task = Create(description: "   ");

        Assert.Equal(string.Empty, task.Description);
    }

    private static TaskItem Create(
        Guid? id = null,
        Guid? ownerId = null,
        string title = "Write the brief",
        string description = "Notes for the demo",
        TaskState status = TaskState.Pending,
        DateOnly? dueDate = null)
    {
        return new TaskItem(
            id ?? TaskId,
            ownerId ?? OwnerId,
            title,
            description,
            status,
            dueDate ?? DueDate);
    }
}
