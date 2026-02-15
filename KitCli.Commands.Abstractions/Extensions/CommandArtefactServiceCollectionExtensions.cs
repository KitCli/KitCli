using System.Reflection;
using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;
using KitCli.Commands.Abstractions.Artefacts.CommandRan;
using KitCli.Commands.Abstractions.Artefacts.Page;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions.Extensions;

public static class CommandArtefactServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddCommandArtefactFactories()
        {
            // TODO: Artefacts - Add Service Collection Extension .AddCommandArtefact<>
            return serviceCollection
                .AddSingleton<IArtefactFactory, RanArtefactFactory>()
                .AddSingleton<IArtefactFactory, PageSizeArtefactFactory>()
                .AddSingleton<IArtefactFactory, PageNumberArtefactFactory>()
                .AddSingleton<IArtefactFactory, AggregatorFilterArtefactFactory>();
        }

        public IServiceCollection AddCommandArtefactFactory<TFactory>()
            where TFactory : class, IArtefactFactory
            => serviceCollection.AddSingleton<IArtefactFactory, TFactory>();

        public IServiceCollection AddAggregatorCommandArtefactsFromAssembly(Assembly? assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "No Assembly Containing ICommand Implementation");
            }
        
            var possibleAggregatorTypes = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && type.BaseType != typeof(object));
        
            foreach (var possibleAggregatorType in possibleAggregatorTypes)
            {
                var aggregatorType = possibleAggregatorType.GetSuperclassGenericOf(typeof(Aggregator<>));
                if (aggregatorType is null)
                {
                    continue;
                }
            
                var typeForReferencedAggregate = aggregatorType.GenericTypeArguments.First();
        
                var strategyType = typeof(AggregatorArtefactFactory<>).MakeGenericType(typeForReferencedAggregate);

                var instance = Activator.CreateInstance(strategyType);
                if (instance is not IArtefactFactory factoryInstance)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of type {strategyType.Name} as ICliCommandPropertyFactory");
                }

                serviceCollection.AddSingleton(factoryInstance);
            }
        
            return serviceCollection;
        }

        public IServiceCollection AddListAggregatorCommandArtefactsFromAssembly(Assembly? assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "No Assembly Containing ICommand Implementation");
            }
        
            var possibleAggregatorTypes = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && type.BaseType != typeof(object));
        
            foreach (var possibleAggregatorType in possibleAggregatorTypes)
            {
                var aggregatorType = possibleAggregatorType.GetSuperclassGenericOf(typeof(ListAggregator<>));
                if (aggregatorType is null)
                {
                    continue;
                }
            
                var typeForReferencedAggregate = aggregatorType.GenericTypeArguments.First();
        
                var strategyType = typeof(ListAggregatorArtefactFactory<>).MakeGenericType(typeForReferencedAggregate);

                var instance = Activator.CreateInstance(strategyType);
                if (instance is not IArtefactFactory factoryInstance)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of type {strategyType.Name} as ICliCommandPropertyFactory");
                }

                serviceCollection.AddSingleton(factoryInstance);
            }
        
            return serviceCollection;
        }
    }
}