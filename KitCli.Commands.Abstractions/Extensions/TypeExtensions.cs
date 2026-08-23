using System.Reflection;

namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// Reflection helpers for filtering and inspecting <see cref="Type"/> values during automatic registration.
/// </summary>
public static class TypeExtensions
{
    extension(IEnumerable<Type> types)
    {
        /// <summary>
        /// Filters the sequence to classes assignable to the given type.
        /// </summary>
        /// <param name="thatImplementType">The type to check assignability against.</param>
        /// <returns>The classes assignable to <paramref name="thatImplementType"/>.</returns>
        public IEnumerable<Type> WhereClassTypesAssignableFrom(Type thatImplementType)
            => types.Where(types => types.IsClass && thatImplementType.IsAssignableFrom(types));
    }

    extension(Type type)
    {
        /// <summary>
        /// Determines whether this type inherits from the given base type, walking up the base-type chain
        /// and comparing generic types by their open generic definition.
        /// </summary>
        /// <param name="baseType">The base type (open generic or not) to check for.</param>
        /// <returns><see langword="true"/> if this type inherits from <paramref name="baseType"/>; otherwise <see langword="false"/>.</returns>
        public bool InheritsFrom(Type baseType)
        {
            var currentBaseType = type;

            while (currentBaseType != null && currentBaseType != typeof(object))
            {
                var inheritedType = currentBaseType.IsGenericType
                    ? currentBaseType.GetGenericTypeDefinition()
                    : currentBaseType;

                if (inheritedType == baseType)
                {
                    return true;
                }

                currentBaseType = currentBaseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Determines whether any of this type's generic type arguments is the given type.
        /// </summary>
        /// <param name="comparingType">The type to look for among the generic type arguments.</param>
        /// <returns><see langword="true"/> if <paramref name="comparingType"/> is one of this type's generic type arguments; otherwise <see langword="false"/>.</returns>
        public bool FirstGenericArgumentIs(Type comparingType)
            => type.IsGenericType &&
               type.GenericTypeArguments.FirstOrDefault(genericType => genericType == comparingType) != null;

        /// <summary>
        /// Gets this type's first generic type argument, if any.
        /// </summary>
        /// <returns>The first generic type argument, or <see langword="null"/> if this type has none.</returns>
        public Type? FirstGenericArgumentOrDefault()
            => type.GenericTypeArguments.FirstOrDefault();

        /// <summary>
        /// Gets every instruction name declared on this type via <see cref="CliCommandAliasAttribute"/>.
        /// </summary>
        /// <returns>The declared alias names, in declaration order.</returns>
        public IEnumerable<string> GetCliCommandAliasNames()
            => type
                .GetCustomAttributes<CliCommandAliasAttribute>(inherit: false)
                .Select(alias => alias.Name);
    }
}