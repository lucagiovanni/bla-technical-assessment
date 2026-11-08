namespace TaskManager.Api.Tests;

public class ApiHostTests
{
    [Fact]
    public void Host_assembly_is_the_api()
    {
        Assert.Equal("TaskManager.Api", typeof(Program).Assembly.GetName().Name);
    }
}
