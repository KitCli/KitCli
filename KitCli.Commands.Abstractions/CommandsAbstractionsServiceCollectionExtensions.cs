using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outputs.Outcomes;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions;

public static class CommandsAbstractionsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommandAbstractions()
        {
            services.AddSingleton<ICliCommandOutcomeIoWriter, CliCommandNotFoundOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, OutputCliCommandOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, MessageCliCommandOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, TableCliCommandOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, PageSizeCliCommandOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, PageNumberCliCommandOutcomeIoWriter>();
            services.AddSingleton<ICliCommandOutcomeIoWriter, ExceptionCliCommandOutcomeIoWriter>(); 
        
            return services;
        }

        public IServiceCollection AddCommandOutcomeIoWriter<TWriter>() 
            where TWriter : class, ICliCommandOutcomeIoWriter
            => services.AddSingleton<ICliCommandOutcomeIoWriter, TWriter>();
    }
}