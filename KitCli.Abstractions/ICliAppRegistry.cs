using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

/// <summary>
/// Registers a CLI application's services with the dependency injection container.
/// </summary>
public interface ICliAppRegistry
{
    /// <summary>
    /// Registers the application's services with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void Register(IServiceCollection services);
}