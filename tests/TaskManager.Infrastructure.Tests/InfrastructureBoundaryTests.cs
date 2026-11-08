namespace TaskManager.Infrastructure.Tests;

public class InfrastructureBoundaryTests
{
    [Fact]
    public void Infrastructure_references_the_application_layer()
    {
        Assert.Equal("TaskManager.Application", TaskManager.Infrastructure.AssemblyReference.Application.GetName().Name);

        var names = TaskManager.Infrastructure.AssemblyReference.Value
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name);

        Assert.Contains("TaskManager.Application", names);
        Assert.DoesNotContain(names, name => name == "TaskManager.Api");
    }
}
