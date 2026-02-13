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
            services.AddSingleton<IOutcomeIoWriter, NotFoundOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, OutputOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, MessageOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, TableOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, PageSizeOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, PageNumberOutcomeIoWriter>();
            services.AddSingleton<IOutcomeIoWriter, ExceptionOutcomeIoWriter>(); 
        
            return services;
        }

        public IServiceCollection AddCommandOutcomeIoWriter<TWriter>() 
            where TWriter : class, IOutcomeIoWriter
            => services.AddSingleton<IOutcomeIoWriter, TWriter>();
    }
}