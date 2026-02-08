using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

public interface ICliAppRegistry
{
    void Register(IServiceCollection services);
}