using KitCli.Abstractions.Io;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

/// <summary>
/// Dependency injection extension methods for registering KitCli abstractions services.
/// </summary>
public static class CliAbstractionsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core KitCli abstractions services, such as <see cref="ICliIo"/>, with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, to allow chaining.</returns>
    public static IServiceCollection AddCliAbstractions(this IServiceCollection services)
    {
        services.AddSingleton<ICliIo, CliIo>();
        return services;
    }
}