using KitCli.Commands.Abstractions.Extensions;
using KitCli.Workflow.Commands.Exit;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow.Commands;

/// <summary>
/// Extension methods for registering the CLI workflow's built-in commands with dependency injection.
/// </summary>
public static class WorkflowCommandsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the commands defined in this assembly (such as <see cref="ExitCliCommand"/>) and the
    /// <see cref="ICliWorkflowCommandProvider"/> used to resolve them.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddCliWorkflowCommands(this IServiceCollection services)
        => services
            .AddCommandsFromAssembly(typeof(ExitCliCommand).Assembly)
            .AddSingleton<ICliWorkflowCommandProvider, CliWorkflowCommandProvider>();
}