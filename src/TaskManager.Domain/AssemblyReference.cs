using System.Reflection;

namespace TaskManager.Domain;

public static class AssemblyReference
{
    public static Assembly Value => typeof(AssemblyReference).Assembly;
}
