using System.Reflection;

namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// Reflection helpers for scanning an assembly's types during automatic registration.
/// </summary>
public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        /// <summary>
        /// Determines whether the assembly contains any class assignable to the given type.
        /// </summary>
        /// <param name="thatImplementType">The type to check assignability against.</param>
        /// <returns><see langword="true"/> if at least one matching class exists; otherwise <see langword="false"/>.</returns>
        public bool AnyClassTypesImplementType(Type thatImplementType)
            => assembly
                .GetTypes()
                .WhereClassTypesAssignableFrom(thatImplementType)
                .Any();

        /// <summary>
        /// Finds every class in the assembly assignable to the given type.
        /// </summary>
        /// <param name="thatImplementType">The type to check assignability against.</param>
        /// <returns>The matching class types.</returns>
        public List<Type> WhereClassTypesImplementType(Type thatImplementType)
            => assembly
                .GetTypes()
                .WhereClassTypesAssignableFrom(thatImplementType)
                .ToList();

        /// <summary>
        /// Finds every non-abstract class in the assembly that inherits from the given open generic type.
        /// </summary>
        /// <param name="thatImplementGenericType">The open generic type to check inheritance against.</param>
        /// <returns>The matching class types.</returns>
        public List<Type> WhereClassTypesImplementGenericType(Type thatImplementGenericType)
            => assembly
                .GetTypes()
                .Where(assemblyType => assemblyType.IsClass &&
                                       !assemblyType.IsAbstract &&
                                       assemblyType.BaseType != null &&
                                       assemblyType.BaseType.IsGenericType &&
                                       assemblyType.InheritsFrom(thatImplementGenericType))
                .ToList();

        /// <summary>
        /// Finds every closed generic base type, for the given open generic type, among the assembly's
        /// non-abstract classes — e.g. every closed <c>Aggregator&lt;TSource, TAggregate&gt;</c> a concrete
        /// aggregator subclass inherits from.
        /// </summary>
        /// <param name="thatImp">The open generic type to look for as a base type.</param>
        /// <returns>The closed generic base types found.</returns>
        public List<Type> AllGenericImplementationsOf(Type thatImp)
            => assembly
                .GetTypes()
                .Where(assemblyType => assemblyType.IsClass &&
                                       !assemblyType.IsAbstract &&
                                       assemblyType.BaseType != null &&
                                       assemblyType.BaseType.IsGenericType)
                .Select(p => p.GetSuperclassGenericOf(thatImp))
                .Where(t => t != null)
                .Select(t => t!)
                .ToList();
    }
}