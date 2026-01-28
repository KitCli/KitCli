using KitCli.Abstractions.Io;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Abstractions;

public static class CliAbstractionsServiceCollectionExtensions
{
    public static IServiceCollection AddCliAbstractions(this IServiceCollection services)
    {
        services.AddSingleton<ICliIo, CliIo>();
        return services;
    }
}