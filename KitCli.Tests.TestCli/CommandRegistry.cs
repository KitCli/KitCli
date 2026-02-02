using KitCli.Abstractions;
using KitCli.Commands.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

public class CommandRegistry : ICliAppBuilderRegistry
{
    public void Register(IServiceCollection services)
    {
        services.AddCommandsFromAssembly(typeof(TestCliCommand).Assembly);
    }
}