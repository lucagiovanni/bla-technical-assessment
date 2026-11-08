using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure.Tests;

public class InfrastructureRegistrationTests
{
    [Fact]
    public void Registers_sqlite_repositories_and_password_hashing()
    {
        using var database = new SqliteDatabase();
        var services = new ServiceCollection();

        services.AddInfrastructure(database.ConnectionString);

        Assert.Contains(services, service => service.ServiceType == typeof(IUserRepository) && service.ImplementationType == typeof(UserRepository));
        Assert.Contains(services, service => service.ServiceType == typeof(ITaskRepository) && service.ImplementationType == typeof(TaskRepository));
        Assert.Contains(services, service => service.ServiceType == typeof(IPasswordHasher) && service.ImplementationType == typeof(Pbkdf2PasswordHasher));
    }
}
