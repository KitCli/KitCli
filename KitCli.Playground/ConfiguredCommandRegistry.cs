using KitCli.Abstractions;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Playground;

public class ConfiguredCommandRegistry : IConfigurableCliAppRegistry<InstructionSettings> 
{
    public void Register(InstructionSettings settings, IServiceCollection services)
    {
        Console.WriteLine("No services registered in ConfiguredCommandRegistry.");
    }
}