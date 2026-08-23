using System.Reflection;
using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Artefacts.RanCliCommand;
using KitCli.Commands.Abstractions.Artefacts.TableBuilder;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// Dependency injection registration helpers for <see cref="IArtefactFactory"/> discovery.
/// </summary>
public static class ArtefactServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the built-in artefact factories, then reflection-scans the given assembly for every
        /// custom <see cref="ArtefactFactory{TOutcome}"/>, every closed <see cref="Aggregator{TSource,TAggregate}"/>
        /// implementation, and every closed <see cref="TableBuilder{TSource,TAggregate}"/> implementation, registering
        /// their corresponding factories automatically.
        /// </summary>
        /// <param name="assembly">The assembly to scan for custom artefact factories, aggregators, and table builders.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public IServiceCollection AddArtefactFactoriesForAssembly(Assembly? assembly)
        {
            if (assembly == null)
            {
                // TODO: Review this.
                throw new ArgumentNullException(nameof(assembly), "No Assembly Provided");
            }
            
            return serviceCollection
                .AddArtefactFactory<RanCliCommandArtefactFactory>()
                .AddArtefactFactory<PageSizeArtefactFactory>()
                .AddArtefactFactory<PageNumberArtefactFactory>()
                .AddArtefactFactory<AggregatorFilterArtefactFactory>()
                .AddCustomArtefactFactoriesForAssembly(assembly)
                .AddAggregatorArtefactFactoriesForAssembly(assembly)
                .AddTableBuilderArtefactFactoriesForAssembly(assembly);
        }

        private IServiceCollection AddArtefactFactory<TFactory>()
            where TFactory : class, IArtefactFactory
            => serviceCollection.AddSingleton<IArtefactFactory, TFactory>();
        
        private IServiceCollection AddCustomArtefactFactoriesForAssembly(Assembly assembly)
        {
            var factoryTypes = assembly.WhereClassTypesImplementGenericType(typeof(ArtefactFactory<>));
        
            foreach (var factoryType in factoryTypes)
            {
                var instance = Activator.CreateInstance(factoryType);
                if (instance is not IArtefactFactory factoryInstance)
                {
                    throw new InvalidOperationException($"Could not create instance of type {factoryType.Name} as IArtefactFactory");
                }

                serviceCollection.AddSingleton(factoryInstance);
            }
        
            return serviceCollection;
        }

        private IServiceCollection AddAggregatorArtefactFactoriesForAssembly(Assembly assembly)
        {
            var aggregatorTypes = assembly.AllGenericImplementationsOf(typeof(Aggregator<,>));
        
            foreach (var aggregatorType in aggregatorTypes)
            {
                var typeForReferencedSource = aggregatorType.GenericTypeArguments[0];
                var typeForReferencedAggregate = aggregatorType.GenericTypeArguments[1];
        
                var factoryType = typeof(AggregatorArtefactFactory<,>)
                    .MakeGenericType(typeForReferencedSource, typeForReferencedAggregate);

                var instance = Activator.CreateInstance(factoryType);
                if (instance is not IArtefactFactory factoryInstance)
                {
                    throw new InvalidOperationException($"Could not create instance of type {factoryType.Name}");
                }

                serviceCollection.AddSingleton(factoryInstance);
            }
        
            return serviceCollection;
        }

        private IServiceCollection AddTableBuilderArtefactFactoriesForAssembly(Assembly assembly)
        {
            var tableBuilderTypes = assembly.AllGenericImplementationsOf(typeof(TableBuilder<,>));
        
            foreach (var tableBuilderType in tableBuilderTypes)
            {
                var typeForReferencedSource = tableBuilderType.GenericTypeArguments[0];
                var typeForReferencedAggregate = tableBuilderType.GenericTypeArguments[1];
        
                var strategyType = typeof(TableBuilderArtefactFactory<,>)
                    .MakeGenericType(typeForReferencedSource, typeForReferencedAggregate);

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