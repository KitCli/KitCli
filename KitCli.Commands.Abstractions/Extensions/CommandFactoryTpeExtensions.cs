namespace KitCli.Commands.Abstractions.Extensions;

internal static class CommandFactoryTpeExtensions
{
    extension(Type current)
    {
        public Type? GetSuperclassGenericOf(Type genericType)
        {
            for (var currentType = current.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericType)
                {
                    return currentType;
                }
            }

            return null;
        }
    }
}