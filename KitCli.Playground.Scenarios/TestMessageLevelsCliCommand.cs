using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Leveled messages. Says one message at every <see cref="MessageLevel"/>, then a plain, always-shown
/// final message. Add <c>.WithMinimumMessageLevel(MessageLevel.Warning)</c> to this app's
/// <c>CliAppBuilder</c> chain in <c>Program.cs</c> and rerun to see the quieter ones disappear — the
/// final message still shows either way, since it isn't leveled. Type <c>/test-message-levels</c>.
/// </summary>
public record TestMessageLevelsCliCommand : CliCommand;

public class TestMessageLevelsCliCommandHandler : CliCommandHandler<TestMessageLevelsCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestMessageLevelsCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("A trace message.", MessageLevel.Trace)
            .BySaying("A debug message.", MessageLevel.Debug)
            .BySaying("An information message.", MessageLevel.Information)
            .BySaying("A warning message.", MessageLevel.Warning)
            .BySaying("An error message.", MessageLevel.Error)
            .ByFinallySaying("Done — the five messages above were leveled; this one always shows.")
            .EndAsync();
}
