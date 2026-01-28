using System.Reflection;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<Type> WhereClassTypesImplement(this IEnumerable<Type> types, Type thatImplementType)
        => types.Where(assemblyType => assemblyType.IsClass && thatImplementType.IsAssignableFrom(assemblyType));
    
    public static Type? FirstOrDefaultGenericInterface(this Type types)
        => types
            .GetInterfaces()
            .FirstOrDefault(interfaceType => interfaceType.GenericTypeArguments.Length != 0);
    
    public static Type FirstOrDefaultGenericTypeArgument(this Type implementationType)
    {
        var genericInterfaceType = implementationType.FirstOrDefaultGenericInterface();

        if (genericInterfaceType is null)
        {
            var implementationTypeName = implementationType.Name;
            var typeName = typeof(ICliCommandFactory<>).Name;
                
            throw new ArgumentException($"Type '{implementationTypeName}' does not implement {typeName} interface");
        }
        
        return genericInterfaceType.GenericTypeArguments.First();
    }
    
    public static FieldInfo GetRequiredField(this Type typeForAssignedCommand, string fieldTypeName)
    {
        var commandNameField = typeForAssignedCommand.GetField(fieldTypeName);
        if (commandNameField is null)
        {
            var typeName = typeForAssignedCommand.Name;
            
            throw new ArgumentException($"Type '{typeName}' does not contain a field named '{fieldTypeName}'");
        }
        
        return commandNameField;
    }
    
    public static Type? GetSuperclassGenericOf(this Type current, Type genericType)
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
    
    public static TAttribute? FirstOrDefaultAttributeOfType<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var attributes = type.GetCustomAttributes(typeof(TAttribute));
        return attributes.OfType<TAttribute>().FirstOrDefault();
    }
}