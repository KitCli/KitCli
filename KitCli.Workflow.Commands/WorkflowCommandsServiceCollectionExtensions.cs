using KitCli.Commands.Abstractions.Extensions;
using KitCli.Workflow.Commands.Exit;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow.Commands;

public static class WorkflowCommandsServiceCollectionExtensions
{
    public static IServiceCollection AddCliWorkflowCommands(this IServiceCollection services)
        => services
            .AddCommandsFromAssembly(typeof(ExitCliCommand).Assembly)
            .AddSingleton<ICliWorkflowCommandProvider, CliWorkflowCommandProvider>();
}