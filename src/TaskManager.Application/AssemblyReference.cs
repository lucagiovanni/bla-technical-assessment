using System.Reflection;

namespace TaskManager.Application;

public static class AssemblyReference
{
    public static Assembly Value => typeof(AssemblyReference).Assembly;

    public static Assembly Domain => typeof(TaskManager.Domain.AssemblyReference).Assembly;
}
