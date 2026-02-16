using System.Reflection;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions.Extensions;

public static class CommandServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommandsFromAssembly(Assembly? assembly) 
        {
            if (assembly == null)
            {
                throw new NullReferenceException("No Assembly Containing ICommand Implementation");
            }

            if (!assembly.AnyClassTypesImplementType(typeof(CliCommand)))
            {
                throw new ArgumentException($"No ICommand Implementations Found in Assembly '{assembly.FullName}'");
            }

            return services
                .AddCommandFactories(assembly)
                .AddMediatRCommandsAndHandlers(assembly);
        }

        private IServiceCollection AddMediatRCommandsAndHandlers(Assembly assembly)
            => services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        private IServiceCollection AddCommandFactories(Assembly assembly)
        {
            var commandImplementationTypes = assembly.WhereClassTypesImplementType(typeof(CliCommand));
            var factoryImplementationTypes = assembly.WhereClassTypesImplementGenericType(typeof(CliCommandFactory<>));

            foreach (var commandType in commandImplementationTypes)
            {
                var matchingFactories = factoryImplementationTypes
                    .Where(factory => factory.BaseType!.FirstGenericArgumentIs(commandType))
                    .ToList();

                if (matchingFactories.Count > 1)
                {
                    throw new ArgumentException($"Multiple factories found for command type '{commandType.Name}'");
                }

                if (matchingFactories.Count == 1)
                {
                    services.AddCommandFactory(commandType, matchingFactories.First());
                    continue;
                }

                var hasEmptyConstructor = commandType.GetConstructor(Type.EmptyTypes) is not null;
                if (hasEmptyConstructor)
                {
                    var basicFactoryType = typeof(BasicCliCommandFactory<>).MakeGenericType(commandType);
                    services.AddCommandFactory(commandType, basicFactoryType);
                }
            }
        
            return services;
        }

        private void AddCommandFactory(Type commandImplementationType, Type factoryImplementationType)
        {
            var specificCommandName = CliCommand.StripCommandName(commandImplementationType.Name);
            
            var commandName = specificCommandName.ToLowerSplitString(InstructionConstants.DefaultCommandNameSeparator);
            var shorthandCommandName = specificCommandName.ToLowerTitleCharacters();
        
            services
                .AddKeyedSingleton(
                    typeof(ICliCommandFactory),
                    commandName,
                    factoryImplementationType)
                .AddKeyedSingleton(
                    typeof(ICliCommandFactory),
                    shorthandCommandName,
                    factoryImplementationType);
        }
    }
}