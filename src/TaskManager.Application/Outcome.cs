namespace TaskManager.Application;

public sealed class Outcome<T> where T : class
{
    private Outcome(T? value, Failure? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Failure? Error { get; }

    public bool IsSuccess => Error is null;

    public static Outcome<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Outcome<T>(value, null);
    }

    public static Outcome<T> Fail(Failure error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Outcome<T>(null, error);
    }
}
