using System.Reflection;

namespace KitCli.Commands.Abstractions.Extensions;

public static class AssemblyExtensions
{
    public static List<Type> WhereClassTypesImplementType(this Assembly assembly, Type thatImplementType)
        => assembly
            .GetTypes()
            .WhereClassTypesImplement(thatImplementType)
            .ToList();
}