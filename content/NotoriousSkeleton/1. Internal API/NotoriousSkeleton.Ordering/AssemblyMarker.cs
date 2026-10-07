using System.Reflection;

namespace NotoriousSkeleton.Ordering;

public static class AssemblyMarker
{
    public static Assembly CurrentAssembly => typeof(AssemblyMarker).Assembly;
}
