using TaskManager.Application.Tasks;

namespace TaskManager.Application.Tests;

public class TaskUseCaseTests
{
    private static readonly DateOnly DueDate = new(2026, 11, 9);
    private readonly Guid _ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _otherOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly TaskRepositoryFake _repository = new();
    private readonly CreateTask _create;
    private readonly GetTask _get;
    private readonly ListTasks _list;
    private readonly UpdateTask _update;
    private readonly DeleteTask _delete;

    public TaskUseCaseTests()
    {
        _create = new CreateTask(_repository);
        _get = new GetTask(_repository);
        _list = new ListTasks(_repository);
        _update = new UpdateTask(_repository);
        _delete = new DeleteTask(_repository);
    }

    [Fact]
    public async Task Create_stores_the_owners_task_fields()
    {
        var created = await _create.ExecuteAsync(_ownerId, "  Write the brief  ", "  Demo notes  ", "inprogress", DueDate, CancellationToken.None);

        var model = AssertSuccess(created);
        Assert.NotEqual(Guid.Empty, model.Id);
        Assert.Equal("Write the brief", model.Title);
        Assert.Equal("Demo notes", model.Description);
        Assert.Equal("InProgress", model.Status);
        Assert.Equal(DueDate, model.DueDate);
        Assert.Equal(_ownerId, Assert.Single(_repository.Items).OwnerId);
    }

    [Fact]
    public async Task Create_allows_an_empty_description_and_a_past_due_date()
    {
        var due = new DateOnly(2020, 1, 2);

        var created = await _create.ExecuteAsync(_ownerId, "Overdue", null, "Completed", due, CancellationToken.None);

        var model = AssertSuccess(created);
        Assert.Equal(string.Empty, model.Description);
        Assert.Equal("Completed", model.Status);
        Assert.Equal(due, model.DueDate);
    }

    [Theory]
    [InlineData(null, "Pending", "Title is required.")]
    [InlineData("   ", "Pending", "Title is required.")]
    [InlineData("Title", null, "Status is not valid.")]
    [InlineData("Title", "Done", "Status is not valid.")]
    [InlineData("Title", "1", "Status is not valid.")]
    public async Task Create_rejects_invalid_fields_without_saving(string? title, string? status, string message)
    {
        var created = await _create.ExecuteAsync(_ownerId, title, "Notes", status, DueDate, CancellationToken.None);

        AssertFailure(created, FailureKind.Validation, message);
        Assert.Equal(0, _repository.AddCount);
    }

    [Fact]
    public async Task Create_requires_a_due_date()
    {
        var created = await _create.ExecuteAsync(_ownerId, "Title", "Notes", "Pending", null, CancellationToken.None);

        AssertFailure(created, FailureKind.Validation, "Due date is required.");
        Assert.Equal(0, _repository.AddCount);
    }

    [Fact]
    public async Task Create_rejects_a_title_or_description_that_is_too_long()
    {
        var longTitle = await _create.ExecuteAsync(
            _ownerId,
            new string('t', 201),
            "Notes",
            "Pending",
            DueDate,
            CancellationToken.None);
        var longDescription = await _create.ExecuteAsync(
            _ownerId,
            "Title",
            new string('d', 4001),
            "Pending",
            DueDate,
            CancellationToken.None);

        AssertFailure(longTitle, FailureKind.Validation, "Title must be 200 characters or fewer.");
        AssertFailure(longDescription, FailureKind.Validation, "Description must be 4000 characters or fewer.");
        Assert.Equal(0, _repository.AddCount);
    }

    [Fact]
    public async Task Create_requires_an_owner()
    {
        var created = await _create.ExecuteAsync(Guid.Empty, "Title", "Notes", "Pending", DueDate, CancellationToken.None);

        AssertFailure(created, FailureKind.Validation, "Owner is required.");
        Assert.Equal(0, _repository.AddCount);
    }

    [Fact]
    public async Task Owner_can_read_update_and_delete_only_their_tasks()
    {
        var mine = AssertSuccess(await _create.ExecuteAsync(_ownerId, "Mine", "A", "Pending", DueDate, CancellationToken.None));
        var theirs = AssertSuccess(await _create.ExecuteAsync(_otherOwnerId, "Theirs", "B", "Pending", DueDate, CancellationToken.None));

        var read = AssertSuccess(await _get.ExecuteAsync(_ownerId, mine.Id, CancellationToken.None));
        Assert.Equal("Mine", read.Title);

        AssertFailure(
            await _get.ExecuteAsync(_ownerId, theirs.Id, CancellationToken.None),
            FailureKind.NotFound,
            "Task was not found.");
        AssertFailure(
            await _update.ExecuteAsync(_ownerId, theirs.Id, "Changed", "B", "Completed", DueDate, CancellationToken.None),
            FailureKind.NotFound,
            "Task was not found.");
        AssertFailure(
            await _delete.ExecuteAsync(_ownerId, theirs.Id, CancellationToken.None),
            FailureKind.NotFound,
            "Task was not found.");

        Assert.Equal("Theirs", _repository.Items.Single(task => task.Id == theirs.Id).Title);
        Assert.Equal(0, _repository.RemoveCount);

        var updated = AssertSuccess(await _update.ExecuteAsync(_ownerId, mine.Id, " Mine updated ", "", "completed", new DateOnly(2026, 12, 1), CancellationToken.None));
        Assert.Equal(mine.Id, updated.Id);
        Assert.Equal("Mine updated", updated.Title);
        Assert.Equal(string.Empty, updated.Description);
        Assert.Equal("Completed", updated.Status);
        Assert.Equal(new DateOnly(2026, 12, 1), updated.DueDate);
        Assert.Equal(_ownerId, _repository.Items.Single(task => task.Id == mine.Id).OwnerId);

        AssertSuccess(await _delete.ExecuteAsync(_ownerId, mine.Id, CancellationToken.None));
        AssertFailure(
            await _get.ExecuteAsync(_ownerId, mine.Id, CancellationToken.None),
            FailureKind.NotFound,
            "Task was not found.");
        Assert.DoesNotContain(_repository.Items, task => task.Id == mine.Id);
    }

    [Fact]
    public async Task Update_rejects_invalid_fields_without_changing_the_task()
    {
        var mine = AssertSuccess(await _create.ExecuteAsync(_ownerId, "Mine", "A", "Pending", DueDate, CancellationToken.None));

        AssertFailure(
            await _update.ExecuteAsync(_ownerId, mine.Id, " ", "A", "Pending", DueDate, CancellationToken.None),
            FailureKind.Validation,
            "Title is required.");

        Assert.Equal("Mine", _repository.Items.Single(task => task.Id == mine.Id).Title);
        Assert.Equal(0, _repository.UpdateCount);
    }

    [Fact]
    public async Task List_returns_only_the_owners_tasks_in_due_date_then_title_order()
    {
        _repository.ReturnEveryOwner = true;
        await _create.ExecuteAsync(_ownerId, "Zebra", "Later", "Pending", new DateOnly(2026, 12, 1), CancellationToken.None);
        await _create.ExecuteAsync(_otherOwnerId, "Hidden", "No", "Pending", new DateOnly(2026, 1, 1), CancellationToken.None);
        await _create.ExecuteAsync(_ownerId, "alpha", "Soon", "Pending", new DateOnly(2026, 11, 1), CancellationToken.None);
        await _create.ExecuteAsync(_ownerId, "Beta", "Same day", "Pending", new DateOnly(2026, 11, 1), CancellationToken.None);

        var tasks = await _list.ExecuteAsync(_ownerId, CancellationToken.None);

        Assert.Equal(["alpha", "Beta", "Zebra"], tasks.Select(task => task.Title).ToArray());
    }

    [Fact]
    public async Task List_is_empty_when_the_owner_is_missing()
    {
        await _create.ExecuteAsync(_ownerId, "Mine", "A", "Pending", DueDate, CancellationToken.None);

        var tasks = await _list.ExecuteAsync(Guid.Empty, CancellationToken.None);

        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Missing_task_cannot_be_read_updated_or_deleted()
    {
        var missing = Guid.NewGuid();

        AssertFailure(await _get.ExecuteAsync(_ownerId, missing, CancellationToken.None), FailureKind.NotFound, "Task was not found.");
        AssertFailure(
            await _update.ExecuteAsync(_ownerId, missing, "Title", "Notes", "Pending", DueDate, CancellationToken.None),
            FailureKind.NotFound,
            "Task was not found.");
        AssertFailure(await _delete.ExecuteAsync(_ownerId, missing, CancellationToken.None), FailureKind.NotFound, "Task was not found.");
        Assert.Equal(0, _repository.UpdateCount);
        Assert.Equal(0, _repository.RemoveCount);
    }

    private static TaskModel AssertSuccess(Outcome<TaskModel> outcome)
    {
        Assert.True(outcome.IsSuccess);
        Assert.NotNull(outcome.Value);
        return outcome.Value;
    }

    private static void AssertFailure(Outcome<TaskModel> outcome, FailureKind kind, string message)
    {
        Assert.False(outcome.IsSuccess);
        Assert.NotNull(outcome.Error);
        Assert.Equal(kind, outcome.Error.Kind);
        Assert.Equal(message, outcome.Error.Message);
    }
}
