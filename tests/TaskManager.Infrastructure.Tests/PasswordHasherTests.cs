using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure.Tests;

public class PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_verifies_the_same_password_and_rejects_a_different_one()
    {
        var hash = _hasher.Hash("Demo123!");
        var again = _hasher.Hash("Demo123!");

        Assert.NotEqual("Demo123!", hash);
        Assert.NotEqual(hash, again);
        Assert.StartsWith("pbkdf2-", hash, StringComparison.Ordinal);
        Assert.True(_hasher.Verify("Demo123!", hash));
        Assert.True(_hasher.Verify("Demo123!", again));
        Assert.False(_hasher.Verify("Wrong123", hash));
        Assert.False(_hasher.Verify("Demo123!", "not-a-hash"));
        Assert.False(_hasher.Verify("", hash));
    }
}
