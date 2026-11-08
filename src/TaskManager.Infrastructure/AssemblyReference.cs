using System.Reflection;

namespace TaskManager.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Value => typeof(AssemblyReference).Assembly;

    public static Assembly Application => typeof(TaskManager.Application.AssemblyReference).Assembly;
}
