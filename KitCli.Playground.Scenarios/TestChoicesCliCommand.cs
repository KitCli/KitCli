using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Arrow-key prompts. Asks for one fruit from a list, then for several, and says what was picked.
/// Move with the arrow keys, tick with the space bar in the second list, confirm with enter. Ctrl+C
/// abandons the prompt. Not for the headless app, which has nobody to ask. Type <c>/test-choices</c>.
/// </summary>
public record TestChoicesCliCommand : CliCommand;

public class TestChoicesCliCommandHandler(ICliIo io) : CliCommandHandler<TestChoicesCliCommand>
{
    private static readonly CliChoice[] Fruits =
    [
        new("Apple", 1),
        new("Banana", 2),
        new("Cherry [red]", 3)
    ];

    public override async Task<Outcome[]> HandleCommand(TestChoicesCliCommand command, CancellationToken cancellationToken)
    {
        var favourite = await io.ChooseAsync("Which fruit is your favourite?", Fruits, cancellationToken);
        var liked = await io.ChooseManyAsync("Which fruits do you like?", Fruits, cancellationToken);

        return FinishThisCommand()
            .BySaying($"Favourite: {favourite?.Label ?? "nothing picked"} (value {favourite?.Value})")
            .ByFinallySaying($"Liked: {string.Join(", ", liked.Select(choice => choice.Label))}")
            .End();
    }
}
