using TaskManager.Application.Users;

namespace TaskManager.Application.Tests;

internal sealed class PasswordHasherFake : IPasswordHasher
{
    public string Hash(string password) => "hash:" + password;

    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
}
