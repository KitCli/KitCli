namespace KitCli.Commands.Abstractions.Extensions;

public static class TypeExtensions
{
    extension(IEnumerable<Type> types)
    {
        public IEnumerable<Type> WhereClassTypesAssignableFrom(Type thatImplementType)
            => types.Where(types => types.IsClass && thatImplementType.IsAssignableFrom(types));
    }

    extension(Type type)
    {
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

        public bool FirstGenericArgumentIs(Type comparingType)
            => type.IsGenericType &&
               type.GenericTypeArguments.FirstOrDefault(genericType => genericType == comparingType) != null;

        public Type? FirstGenericArgumentOrDefault()
            => type.GenericTypeArguments.FirstOrDefault();
    }
}