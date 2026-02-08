using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

public interface IConfigurableCliAppRegistry<in TSettings> where TSettings : class
{
    void Register(TSettings settings, IServiceCollection services);
}