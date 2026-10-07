using System.Reflection;

namespace NotoriousSkeleton.Catalog;

public static class AssemblyMarker
{
    public static Assembly CurrentAssembly => typeof(AssemblyMarker).Assembly;
}
