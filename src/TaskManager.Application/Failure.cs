namespace TaskManager.Application;

public enum FailureKind
{
    Validation,
    Conflict,
    Unauthorized,
    NotFound
}

public sealed record Failure(FailureKind Kind, string Message)
{
    public static Failure Validation(string message) => new(FailureKind.Validation, message);

    public static Failure Conflict(string message) => new(FailureKind.Conflict, message);

    public static Failure Unauthorized(string message) => new(FailureKind.Unauthorized, message);

    public static Failure NotFound(string message) => new(FailureKind.NotFound, message);
}
