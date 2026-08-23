using System.Reflection;
using KitCli.Abstractions;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Instructions.Extensions;
using KitCli.Workflow;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli;

/// <summary>
/// DI registration entry point that wires up a KitCli app: the abstraction/instruction/command
/// building blocks, the workflow, the built-in workflow commands, every <see cref="Commands.Abstractions.CliCommand"/>
/// found in the entry assembly, and the concrete <see cref="CliApp"/> that drives them.
/// </summary>
public static class CliServiceCollectionExtensions
{
    /// <summary>
    /// Registers the abstractions, instruction parsing, command handling, and workflow services a
    /// KitCli app needs, scans the entry assembly for commands, and registers <typeparamref name="TCliApp"/>
    /// as the singleton <see cref="CliApp"/> to run.
    /// </summary>
    /// <typeparam name="TCliApp">The concrete <see cref="CliApp"/> subclass to run — determines whether the app is terminal- or args-driven.</typeparam>
    /// <param name="serviceCollection">The service collection to register services into.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddCli<TCliApp>(this IServiceCollection serviceCollection) where TCliApp : CliApp
    {
        serviceCollection.AddCliAbstractions();
        serviceCollection.AddCliInstructions();
        serviceCollection.AddCommandAbstractions();
        
        serviceCollection.AddSingleton<ICliWorkflow, CliWorkflow>();
        
        serviceCollection.AddCliWorkflowCommands();
        serviceCollection.AddCommandsFromAssembly(Assembly.GetEntryAssembly());
        
        serviceCollection.AddSingleton<CliApp, TCliApp>();

        return serviceCollection;
    }
}