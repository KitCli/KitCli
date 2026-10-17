# Choosing from a list

## What this is for

Ask the person to pick from a list with the arrow keys, rather than make
them type a name or number back. `ChooseAsync` picks one item and
`ChooseManyAsync` picks several.

## How to do it

Inject `ICliIo` into the handler and hand it a list of `CliChoice`. Each has
a `Label` to show and an optional `Value` handed back with the pick:

```csharp
public class TestChoicesCliCommandHandler(ICliIo io) : CliCommandHandler<TestChoicesCliCommand>
{
    private static readonly CliChoice[] Fruits =
    [
        new("Apple", 1),
        new("Banana", 2),
        new("Cherry", 3)
    ];

    public override async Task<Outcome[]> HandleCommand(TestChoicesCliCommand command, CancellationToken ct)
    {
        var favourite = await io.ChooseAsync("Which fruit is your favourite?", Fruits, ct);
        var liked = await io.ChooseManyAsync("Which fruits do you like?", Fruits, ct);

        return FinishThisCommand()
            .BySaying($"Favourite: {favourite?.Label}")
            .ByFinallySaying($"Liked: {string.Join(", ", liked.Select(choice => choice.Label))}")
            .End();
    }
}
```

That is the playground's `/test-choices`. Move with the up and down arrows
and press enter. In the second list, tick with the space bar first.

Cast `Value` back to what you stored. If the person presses Ctrl+C,
`ChooseAsync` returns `null` and `ChooseManyAsync` returns an empty list;
pressing enter with nothing ticked returns an empty list too.

## Common mistakes

**Printing while the list is showing.** A line written then scrambles the
list. Print before you ask, or after the answer.

**Passing an empty list.** It throws `InvalidOperationException`. Check
first.

**Asking from a headless app.** Nobody is there to answer, so the app breaks.

## Learn more

[../concepts/cli-io.md](../concepts/cli-io.md) ·
[../adr/ask-through-cli-io-with-cli-choice.md](../adr/ask-through-cli-io-with-cli-choice.md)
