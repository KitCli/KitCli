using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A factory that surfaces <see cref="CliCommandFactory{TCliCommand}"/>'s protected argument and artefact
/// resolution to a test, under the same names the API declares them with.
/// </summary>
public class TestCliCommandFactory : CliCommandFactory<TestNextCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create() => new TestNextCliCommand();

    public new Instruction Instruction => base.Instruction;

    public new List<AnonymousArtefact> Artefacts => base.Artefacts;

    public new bool SubCommandIs(string subCommandName) => base.SubCommandIs(subCommandName);

    public new bool IsSubCommand<TCommand>() where TCommand : CliCommand => base.IsSubCommand<TCommand>();

    public new bool AnyArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
        => base.AnyArgument<TArgumentType>(argumentName);

    public new InstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName)
        where TArgumentType : notnull
        => base.GetArgument<TArgumentType>(argumentName);

    public new InstructionArgument<TArgumentType> GetRequiredArgument<TArgumentType>(string? argumentName)
        where TArgumentType : notnull
        => base.GetRequiredArgument<TArgumentType>(argumentName);

    public new IEnumerable<InstructionArgument<TArgumentType>> GetArguments<TArgumentType>()
        where TArgumentType : notnull
        => base.GetArguments<TArgumentType>();

    public new bool AnyArtefact<TArtefactType>(string? artefactName) where TArtefactType : notnull
        => base.AnyArtefact<TArtefactType>(artefactName);

#pragma warning disable CS0618
    public new bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand
        => base.LastCommandWas<TRanCliCommand>();
#pragma warning restore CS0618

    public new bool AnyLastCommandIs<TRanCliCommand>() where TRanCliCommand : CliCommand
        => base.AnyLastCommandIs<TRanCliCommand>();

    public new bool LastCommandIs<TRanCliCommand>() where TRanCliCommand : CliCommand
        => base.LastCommandIs<TRanCliCommand>();

    public new bool LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2>()
        where TRanCliCommand1 : CliCommand
        where TRanCliCommand2 : CliCommand
        => base.LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2>();

    public new bool LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3>()
        where TRanCliCommand1 : CliCommand
        where TRanCliCommand2 : CliCommand
        where TRanCliCommand3 : CliCommand
        => base.LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3>();

    public new bool LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3, TRanCliCommand4>()
        where TRanCliCommand1 : CliCommand
        where TRanCliCommand2 : CliCommand
        where TRanCliCommand3 : CliCommand
        where TRanCliCommand4 : CliCommand
        => base.LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3, TRanCliCommand4>();

    public new bool LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3, TRanCliCommand4, TRanCliCommand5>()
        where TRanCliCommand1 : CliCommand
        where TRanCliCommand2 : CliCommand
        where TRanCliCommand3 : CliCommand
        where TRanCliCommand4 : CliCommand
        where TRanCliCommand5 : CliCommand
        => base.LastCommandIsOneOf<TRanCliCommand1, TRanCliCommand2, TRanCliCommand3, TRanCliCommand4, TRanCliCommand5>();

    public new Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName = null)
        where TArtefactType : notnull
        => base.GetArtefact<TArtefactType>(artefactName);

    public new Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName = null)
        where TArtefactType : notnull
        => base.GetRequiredArtefact<TArtefactType>(artefactName);

    public new Artefact<Aggregator<TSource, TAggregate>>? GetAggregatorArtefact<TSource, TAggregate>(
        string? artefactName = null)
        => base.GetAggregatorArtefact<TSource, TAggregate>(artefactName);

    public new Artefact<Aggregator<TSource, TAggregate>> GetRequiredAggregatorArtefact<TSource, TAggregate>(
        string? artefactName = null)
        => base.GetRequiredAggregatorArtefact<TSource, TAggregate>(artefactName);

    public new IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>() where TArtefactType : notnull
        => base.GetArtefacts<TArtefactType>();
}
