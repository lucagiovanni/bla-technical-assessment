using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly CreateTask _create;
    private readonly GetTask _get;
    private readonly ListTasks _list;
    private readonly UpdateTask _update;
    private readonly DeleteTask _delete;

    public TasksController(CreateTask create, GetTask get, ListTasks list, UpdateTask update, DeleteTask delete)
    {
        _create = create;
        _get = get;
        _list = list;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        return Ok(await _list.ExecuteAsync(ownerId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _get.ExecuteAsync(ownerId, id, cancellationToken);
        return From(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskBody? body, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _create.ExecuteAsync(ownerId, body?.Title, body?.Description, body?.Status, body?.DueDate, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.Validation("Title is required."));
        }

        return Created($"/api/tasks/{result.Value.Id}", result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskBody? body, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _update.ExecuteAsync(ownerId, id, body?.Title, body?.Description, body?.Status, body?.DueDate, cancellationToken);
        return From(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _delete.ExecuteAsync(ownerId, id, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.NotFound("Task was not found."));
        }

        return NoContent();
    }

    private IActionResult From(Outcome<TaskModel> result)
    {
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.NotFound("Task was not found."));
        }

        return Ok(result.Value);
    }
}

public sealed record TaskBody(string? Title, string? Description, string? Status, DateOnly? DueDate);
