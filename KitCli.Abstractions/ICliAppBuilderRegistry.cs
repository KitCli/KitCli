using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

public interface ICliAppBuilderRegistry
{
    void Register(IServiceCollection services);
}