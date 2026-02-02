using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

public interface ICliAppBuilderConfigurableRegistry<in TSettings> where TSettings : class
{
    void Register(TSettings settings, IServiceCollection services);
}