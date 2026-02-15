using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;
using KitCli.Instructions.Parsers;
using KitCli.Instructions.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Instructions.Extensions;

public static class ServiceCollectionExtensions
{
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