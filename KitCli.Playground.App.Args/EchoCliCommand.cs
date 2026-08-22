using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.App.Args;

public record EchoCliCommand(string Text) : CliCommand;

public class EchoCliCommandFactory : CliCommandFactory<EchoCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var argsText = Instruction.Arguments.Count == 0
            ? "(no arguments)"
            : string.Join(", ", Instruction.Arguments.Select(argument => argument.ToString()));

        return new EchoCliCommand($"/echo received: {argsText}");
    }
}

public class EchoCliCommandHandler : CliCommandHandler<EchoCliCommand>
{
    public override Task<Outcome[]> HandleCommand(EchoCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying(command.Text)
            .EndAsync();
}
