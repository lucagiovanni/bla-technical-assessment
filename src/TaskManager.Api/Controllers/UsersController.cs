using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Authentication;
using TaskManager.Application;
using TaskManager.Application.Users;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly RegisterUser _register;
    private readonly LoginUser _login;
    private readonly GetCurrentUser _current;
    private readonly JwtTokenIssuer _tokens;

    public UsersController(RegisterUser register, LoginUser login, GetCurrentUser current, JwtTokenIssuer tokens)
    {
        _register = register;
        _login = login;
        _current = current;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CredentialsBody? body, CancellationToken cancellationToken)
    {
        var result = await _register.ExecuteAsync(body?.Email, body?.Password, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.Validation("Email is required."));
        }

        return Created($"/api/users/{result.Value.Id}", new UserResponse(result.Value.Id, result.Value.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CredentialsBody? body, CancellationToken cancellationToken)
    {
        var result = await _login.ExecuteAsync(body?.Email, body?.Password, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.Unauthorized(LoginUser.InvalidCredentialsMessage));
        }

        return Ok(new LoginResponse(_tokens.Issue(result.Value.Id, result.Value.Email), result.Value.Id, result.Value.Email));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = CurrentUser.Id(User);
        if (userId is null)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _current.ExecuteAsync(userId.Value, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.Unauthorized(GetCurrentUser.UnauthenticatedMessage));
        }

        return Ok(new UserResponse(result.Value.Id, result.Value.Email));
    }
}

public sealed record CredentialsBody(string? Email, string? Password);

public sealed record UserResponse(Guid Id, string Email);

public sealed record LoginResponse(string Token, Guid Id, string Email);
