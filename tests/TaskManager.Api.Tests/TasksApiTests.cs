using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Api.Controllers;
using TaskManager.Application.Tasks;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Tests;

public class TasksApiTests : IClassFixture<ApiFactory>
{
    private static readonly DateOnly DueDate = new(2026, 11, 9);
    private readonly ApiFactory _factory;

    public TasksApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Owner_can_create_read_update_and_delete_a_task()
    {
        using var client = await ClientForAsync($"owner-{Guid.NewGuid():N}@example.com");
        var created = await CreateAsync(client, "  Write the brief  ", "  Demo notes  ", "inprogress", DueDate);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Write the brief", created.Title);
        Assert.Equal("Demo notes", created.Description);
        Assert.Equal("InProgress", created.Status);
        Assert.Equal(DueDate, created.DueDate);

        var listed = await client.GetFromJsonAsync<TaskModel[]>("/api/tasks");
        Assert.NotNull(listed);
        Assert.Equal(created.Id, Assert.Single(listed).Id);

        var fetched = await client.GetFromJsonAsync<TaskModel>($"/api/tasks/{created.Id}");
        Assert.Equal(created.Title, fetched?.Title);

        var update = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = "Write the notes",
            description = "",
            status = "Completed",
            dueDate = "2026-12-01"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<TaskModel>();
        Assert.Equal("Write the notes", updated?.Title);
        Assert.Equal(string.Empty, updated?.Description);
        Assert.Equal("Completed", updated?.Status);
        Assert.Equal(new DateOnly(2026, 12, 1), updated?.DueDate);

        var deleted = await client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var afterDelete = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        var remaining = await client.GetFromJsonAsync<TaskModel[]>("/api/tasks");
        Assert.NotNull(remaining);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Each_verb_rejects_missing_and_invalid_tokens()
    {
        var id = Guid.NewGuid();
        var body = new { title = "Title", description = "Notes", status = "Pending", dueDate = "2026-11-09" };

        using var anonymous = _factory.CreateClient();
        await AssertUnauthorizedAsync(anonymous, id, body);

        using var invalid = _factory.CreateClient();
        invalid.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-token");
        await AssertUnauthorizedAsync(invalid, id, body);
    }

    [Fact]
    public async Task A_person_cannot_read_or_change_someone_elses_task()
    {
        using var owner = await ClientForAsync($"ada-{Guid.NewGuid():N}@example.com");
        using var other = await ClientForAsync($"bea-{Guid.NewGuid():N}@example.com");
        var task = await CreateAsync(owner, "Private", "Mine", "Pending", DueDate);

        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/tasks/{task.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsJsonAsync($"/api/tasks/{task.Id}", new
        {
            title = "Stolen",
            description = "Nope",
            status = "Completed",
            dueDate = "2026-11-09"
        })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/tasks/{task.Id}")).StatusCode);

        var stillThere = await owner.GetFromJsonAsync<TaskModel>($"/api/tasks/{task.Id}");
        Assert.Equal("Private", stillThere?.Title);
        var otherList = await other.GetFromJsonAsync<TaskModel[]>("/api/tasks");
        Assert.NotNull(otherList);
        Assert.Empty(otherList);
    }

    [Fact]
    public async Task Invalid_task_fields_are_rejected()
    {
        using var client = await ClientForAsync($"invalid-{Guid.NewGuid():N}@example.com");

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = " ",
            description = "Notes",
            status = "Done",
            dueDate = "2026-11-09"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var listed = await client.GetFromJsonAsync<TaskModel[]>("/api/tasks");
        Assert.NotNull(listed);
        Assert.Empty(listed);
    }

    [Fact]
    public async Task Demo_user_sees_only_the_seeded_tasks()
    {
        using var client = await ClientForAsync(DemoSeed.Email, DemoSeed.Password, register: false);

        var tasks = await client.GetFromJsonAsync<TaskModel[]>("/api/tasks");

        Assert.NotNull(tasks);
        Assert.Equal(
            ["Prepare the demo", "Review the project brief", "Sketch the task board"],
            tasks.Select(task => task.Title).ToArray());
        Assert.All(tasks, task => Assert.False(string.IsNullOrWhiteSpace(task.Description)));
        Assert.Contains(tasks, task => task.Status == "Pending" && task.DueDate == new DateOnly(2026, 11, 9));
    }

    private async Task AssertUnauthorizedAsync(HttpClient client, Guid id, object body)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/tasks")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/tasks/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/tasks", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync($"/api/tasks/{id}", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync($"/api/tasks/{id}")).StatusCode);
    }

    private async Task<TaskModel> CreateAsync(HttpClient client, string title, string? description, string status, DateOnly dueDate)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new { title, description, status, dueDate });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<TaskModel>();
        Assert.NotNull(created);
        return created;
    }

    private async Task<HttpClient> ClientForAsync(string email, string password = "Secret123", bool register = true)
    {
        var client = _factory.CreateClient();
        if (register)
        {
            var registered = await client.PostAsJsonAsync("/api/users/register", new { email, password });
            Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        }

        var login = await client.PostAsJsonAsync("/api/users/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var signedIn = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(signedIn);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signedIn.Token);
        return client;
    }
}
