using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

/// <summary>
/// Registers a CLI application's services with the dependency injection container, driven by an application-supplied
/// settings object.
/// </summary>
/// <typeparam name="TSettings">The type of the settings object used to configure registration.</typeparam>
public interface IConfigurableCliAppRegistry<in TSettings> where TSettings : class
{
    /// <summary>
    /// Registers the application's services with the service collection, using the supplied settings.
    /// </summary>
    /// <param name="settings">The settings that configure how services are registered.</param>
    /// <param name="services">The service collection to register services with.</param>
    void Register(TSettings settings, IServiceCollection services);
}