namespace TaskManager.Domain.Tests;

public class SolutionLayoutTests
{
    [Fact]
    public void Solution_lists_each_layer_and_its_test_project()
    {
        var root = FindRepositoryRoot();
        var solution = File.ReadAllText(Path.Combine(root, "TaskManager.slnx")).Replace('\\', '/');

        string[] projects =
        [
            "src/TaskManager.Domain/TaskManager.Domain.csproj",
            "src/TaskManager.Application/TaskManager.Application.csproj",
            "src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj",
            "src/TaskManager.Api/TaskManager.Api.csproj",
            "tests/TaskManager.Domain.Tests/TaskManager.Domain.Tests.csproj",
            "tests/TaskManager.Application.Tests/TaskManager.Application.Tests.csproj",
            "tests/TaskManager.Infrastructure.Tests/TaskManager.Infrastructure.Tests.csproj",
            "tests/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj",
        ];

        foreach (var project in projects)
        {
            Assert.Contains(project, solution, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Domain_does_not_reference_other_layers()
    {
        var names = TaskManager.Domain.AssemblyReference.Value
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name);

        Assert.DoesNotContain(names, name => name != null && name.StartsWith("TaskManager.", StringComparison.Ordinal));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaskManager.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
