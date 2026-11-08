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

    [Fact]
    public void Application_does_not_reference_the_database_or_aspnet()
    {
        var names = TaskManager.Application.AssemblyReference.Value
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name);

        Assert.DoesNotContain(names, name => name != null && (
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
            name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) ||
            name.StartsWith("Microsoft.Data.Sqlite", StringComparison.Ordinal)));
    }
}
