using KitCli.Commands.Abstractions.Io;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions;

/// <summary>
/// Dependency injection registration helpers for the command abstractions layer.
/// </summary>
public static class CommandsAbstractionsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the built-in <see cref="IOutcomeIoWriter"/> implementations.
        /// </summary>
        /// <returns>The same service collection, for chaining.</returns>
        public IServiceCollection AddCommandAbstractions()
            => services
                .AddOutcomeIoWriter<NotFoundOutcomeIoWriter>()
                .AddOutcomeIoWriter<OutputOutcomeIoWriter>()
                .AddOutcomeIoWriter<MessageOutcomeIoWriter>()
                .AddOutcomeIoWriter<TableOutcomeIoWriter>()
                .AddOutcomeIoWriter<PageSizeOutcomeIoWriter>()
                .AddOutcomeIoWriter<PageNumberOutcomeIoWriter>()
                .AddOutcomeIoWriter<SuggestionOutcomeIoWriter>()
                .AddOutcomeIoWriter<ExceptionOutcomeIoWriter>();

        /// <summary>
        /// Registers a singleton <see cref="IOutcomeIoWriter"/> implementation.
        /// </summary>
        /// <typeparam name="TWriter">The writer type to register.</typeparam>
        /// <returns>The same service collection, for chaining.</returns>
        public IServiceCollection AddOutcomeIoWriter<TWriter>() where TWriter : class, IOutcomeIoWriter
            => services.AddSingleton<IOutcomeIoWriter, TWriter>();
    }
}