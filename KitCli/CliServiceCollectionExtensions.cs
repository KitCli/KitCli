using KitCli.Abstractions;
using KitCli.Commands.Abstractions.Io.Outcomes;
using KitCli.Instructions.Extensions;
using KitCli.Workflow;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli;

public static class CliServiceCollectionExtensions
{
    public static IServiceCollection AddCli<TCliApp>(this IServiceCollection serviceCollection) where TCliApp : CliApp
    {
        serviceCollection.AddCliAbstractions();
        serviceCollection.AddCliInstructions();
        
        serviceCollection.AddCliWorkflowCommands();
        
        serviceCollection.AddSingleton<ICliWorkflow, CliWorkflow>();
        
        serviceCollection.AddSingleton<ICliCommandOutcomeIo, CliCommandOutcomeIo>();
        
        serviceCollection.AddSingleton<CliApp, TCliApp>();
        
        return serviceCollection;
    }
}