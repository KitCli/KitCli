using System.Reflection;

namespace KitCli.Commands.Abstractions.Extensions;

public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        public bool AnyClassTypesImplementType(Type thatImplementType)
            => assembly
                .GetTypes()
                .WhereClassTypesAssignableFrom(thatImplementType)
                .Any();
        
        public List<Type> WhereClassTypesImplementType(Type thatImplementType)
            => assembly
                .GetTypes()
                .WhereClassTypesAssignableFrom(thatImplementType)
                .ToList();

        public List<Type> WhereClassTypesImplementGenericType(Type thatImplementGenericType)
            => assembly
                .GetTypes()
                .Where(assemblyType => assemblyType.IsClass && 
                                       !assemblyType.IsAbstract && 
                                       assemblyType.BaseType != null && 
                                       assemblyType.BaseType.IsGenericType && 
                                       assemblyType.InheritsFrom(thatImplementGenericType))
                .ToList();
    }
}