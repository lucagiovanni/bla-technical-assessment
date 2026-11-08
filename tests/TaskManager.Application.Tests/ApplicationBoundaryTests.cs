namespace TaskManager.Application.Tests;

public class ApplicationBoundaryTests
{
    [Fact]
    public void Application_references_the_domain_and_not_the_outer_layers()
    {
        var names = TaskManager.Application.AssemblyReference.Value
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => name != null && name.StartsWith("TaskManager.", StringComparison.Ordinal))
            .ToList();

        Assert.Equal("TaskManager.Domain", Assert.Single(names));
        Assert.Equal("TaskManager.Domain", TaskManager.Application.AssemblyReference.Domain.GetName().Name);
    }
}
