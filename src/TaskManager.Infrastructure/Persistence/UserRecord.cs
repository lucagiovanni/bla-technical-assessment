namespace TaskManager.Infrastructure.Persistence;

public sealed class UserRecord
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}
