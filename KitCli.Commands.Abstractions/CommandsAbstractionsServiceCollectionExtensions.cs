using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outputs.Outcomes;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions;

public static class CommandsAbstractionsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommandAbstractions()
            => services
                .AddOutcomeIoWriter<NotFoundOutcomeIoWriter>()
                .AddOutcomeIoWriter<OutputOutcomeIoWriter>() 
                .AddOutcomeIoWriter<MessageOutcomeIoWriter>()
                .AddOutcomeIoWriter<TableOutcomeIoWriter>() 
                .AddOutcomeIoWriter<PageSizeOutcomeIoWriter>()
                .AddOutcomeIoWriter<PageNumberOutcomeIoWriter>()
                .AddOutcomeIoWriter<ExceptionOutcomeIoWriter>();

        public IServiceCollection AddOutcomeIoWriter<TWriter>() where TWriter : class, IOutcomeIoWriter
            => services.AddSingleton<IOutcomeIoWriter, TWriter>();
    }
}