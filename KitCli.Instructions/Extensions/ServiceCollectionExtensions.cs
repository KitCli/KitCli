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
            .AddSingleton<ICliInstructionParser, CliInstructionParser>()
            .AddValidators();
    
    private static IServiceCollection AddCliInstructionArgumentBuilders(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<ICliInstructionArgumentBuilder, DirectoryInfoCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, GuidCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, StringCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, IntCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, DecimalCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, DateOnlyCliInstructionArgumentBuilder>()
            .AddSingleton<ICliInstructionArgumentBuilder, BoolCliInstructionArgumentBuilder>();

    private static IServiceCollection AddTokenExtraction(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<CliInstructionTokenIndexer>()
            .AddSingleton<CliInstructionTokenExtractor>();
    
    private static IServiceCollection AddValidators(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<ICliInstructionValidator, DefaultCliInstructionValidator>();
}