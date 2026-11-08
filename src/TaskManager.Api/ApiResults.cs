using Microsoft.AspNetCore.Mvc;
using TaskManager.Application;

namespace TaskManager.Api;

public sealed record ApiMessage(string Message);

public static class ApiResults
{
    public static IActionResult From(Failure failure)
    {
        var message = new ApiMessage(failure.Message);
        return failure.Kind switch
        {
            FailureKind.Validation => new BadRequestObjectResult(message),
            FailureKind.Conflict => new ConflictObjectResult(message),
            FailureKind.Unauthorized => new UnauthorizedObjectResult(message),
            FailureKind.NotFound => new NotFoundObjectResult(message),
            _ => new ObjectResult(message) { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }
}
