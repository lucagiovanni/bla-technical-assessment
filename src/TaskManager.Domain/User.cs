namespace TaskManager.Domain;

public sealed class User
{
    public Guid Id { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public User(Guid id, string email, string passwordHash)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        Id = id;
        Email = email.Trim();
        PasswordHash = passwordHash;
    }
}
