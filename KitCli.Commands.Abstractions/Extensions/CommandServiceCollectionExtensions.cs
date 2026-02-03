using System.Reflection;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions.Extensions;

public static class CommandServiceCollectionExtensions
{
    public static IServiceCollection AddCommandsFromAssembly(this IServiceCollection serviceCollection, Assembly? assembly) 
    {
        if (assembly == null)
        {
            throw new NullReferenceException("No Assembly Containing ICommand Implementation");
        }

        return serviceCollection
            .AddCommandArtefactFactories()
            .AddCommandGenerators(assembly)
            .AddMediatRCommandsAndHandlers(assembly);
    }

    private static IServiceCollection AddMediatRCommandsAndHandlers(this IServiceCollection serviceCollection, Assembly assembly)
        => serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

    private static IServiceCollection AddCommandGenerators(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var implementationTypes = assembly.WhereClassTypesImplementType(typeof(IUnidentifiedCliCommandFactory));
        
        foreach (var implementationType in implementationTypes)
        {
            var generatorCommandType = implementationType.FirstOrDefaultGenericTypeArgument();
            var specificCommandName = CliCommand.StripCommandName(generatorCommandType.Name);
            
            var commandName = specificCommandName.ToLowerSplitString(CliInstructionConstants.DefaultCommandNameSeparator);
            var shorthandCommandName = specificCommandName.ToLowerTitleCharacters();

            serviceCollection
                .AddKeyedSingleton(
                    typeof(IUnidentifiedCliCommandFactory),
                    commandName,
                    implementationType)
                .AddKeyedSingleton(
                    typeof(IUnidentifiedCliCommandFactory),
                    shorthandCommandName,
                    implementationType);
        }
        
        return serviceCollection;
    }
}