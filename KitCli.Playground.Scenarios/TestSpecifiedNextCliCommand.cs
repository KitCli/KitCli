using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Arguments;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

// Chaining by type, as against TestNextCliCommand's chaining by instance. This handler never builds
// the next command: it names the type, and TestSpecifiedNextResultCliCommandFactory builds it when
// the run gets there - reading the artefact this command produced, which a handler passing an
// instance could only have passed by constructor.
//
// TestSpecifiedNextResultCliCommand has no parameterless constructor, which is the point: naming a
// type does not restrict you to commands that can be built with new(). The factory is the
// constructor, and it supplies the argument.
//
// It reads two things: an artefact this command produced, and an instruction argument this command
// passed on. Neither reaches it any other way once the user has stopped typing.

public record TestSpecifiedNextCliCommand : CliCommand;

public class TestSpecifiedNextCliCommandHandler : CliCommandHandler<TestSpecifiedNextCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSpecifiedNextCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("Specified Next Command Ran (0)")
            .ByResultingIn(new TestOutcome("I was gathered by the run"))
            .ByMovingToCommand<TestSpecifiedNextResultCliCommand>(
                new InstructionArgument<string>("said", "and I was passed as an argument"))
            .EndAsync();
}

public record TestSpecifiedNextResultCliCommand(string Text) : CliCommand;

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
