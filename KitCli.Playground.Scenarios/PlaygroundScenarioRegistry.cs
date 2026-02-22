using KitCli.Abstractions;
using KitCli.Commands.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Playground.Scenarios;

public class PlaygroundScenarioRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
    {
        services.AddCommandsFromAssembly(typeof(OtherTestCliCommand).Assembly);
        services.AddArtefactFactoriesForAssembly(typeof(OtherTestCliCommand).Assembly);
    }
}