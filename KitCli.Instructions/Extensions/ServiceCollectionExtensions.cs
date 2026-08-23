using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;
using KitCli.Instructions.Parsers;
using KitCli.Instructions.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Instructions.Extensions;

/// <summary>
/// Provides extension methods for registering the CLI instruction parsing pipeline with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the instruction argument builders, token extraction, parser, and validators required to parse
    /// terminal input into <see cref="KitCli.Instructions.Abstractions.Instruction"/> instances.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add registrations to.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddCliInstructions(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddCliInstructionArgumentBuilders()
            .AddTokenExtraction()
            .AddSingleton<IInstructionParser, InstructionParser>()
            .AddValidators();
    
    private static IServiceCollection AddCliInstructionArgumentBuilders(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<IInstructionArgumentBuilder, DirectoryInfoInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, GuidInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, StringInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, IntInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, DecimalInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, DateOnlyInstructionArgumentBuilder>()
            .AddSingleton<IInstructionArgumentBuilder, BoolInstructionArgumentBuilder>();

    private static IServiceCollection AddTokenExtraction(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<InstructionTokenIndexer>()
            .AddSingleton<InstructionTokenExtractor>();
    
    private static IServiceCollection AddValidators(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<IInstructionValidator, DefaultInstructionValidator>();
}