using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Arguments;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Chains by type, as against <see cref="TestNextCliCommand"/>'s chaining by instance. Its handler never
/// builds the next command; it names the type, and
/// <see cref="TestSpecifiedNextResultCliCommandFactory"/> builds it when the run gets there.
/// </summary>
public record TestSpecifiedNextCliCommand : CliCommand;

public class TestSpecifiedNextCliCommandHandler : CliCommandHandler<TestSpecifiedNextCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSpecifiedNextCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("Specified Next Command Ran (0)")
            .ByResultingIn(new TestOutcome("I was gathered by the run"))
            .ByMovingToCommand<TestSpecifiedNextResultCliCommand>(
                new NextCliCommandArgument<string>("said", "and I was passed as an argument"))
            .EndAsync();
}

/// <summary>
/// Has no parameterless constructor, which is the point: naming a type does not restrict a chain to
/// commands that can be built with <c>new()</c>.
/// </summary>
public record TestSpecifiedNextResultCliCommand(string Text) : CliCommand;

/// <summary>
/// The constructor for a chained command, reading both of the things that reach one once the user has
/// stopped typing: an artefact the previous command produced, and an argument it passed on.
/// </summary>
public class TestSpecifiedNextResultCliCommandFactory : BasicCreationCliCommandFactory<TestSpecifiedNextResultCliCommand>
{
    public override CliCommand Create()
    {
        var testArtefact = GetRequiredArtefact<string>(nameof(TestArtefact));
        var said = GetRequiredArgument<string>("said");

        return new TestSpecifiedNextResultCliCommand($"{testArtefact.Value}, {said.Value}");
    }
}

public class TestSpecifiedNextResultCliCommandHandler : CliCommandHandler<TestSpecifiedNextResultCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSpecifiedNextResultCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("Specified Next Result Command Ran (1)")
            .ByFinallySaying(command.Text)
            .EndAsync();
}
