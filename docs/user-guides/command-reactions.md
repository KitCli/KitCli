# Running side effects with command reactions

## What this is for

Sometimes a command needs to trigger a side effect that isn't part of
what the user sees as the result — logging an audit event, invalidating
a cache, notifying some other part of the app — and you don't want
every interested party crammed into the command handler itself, or
the handler's own outcome list cluttered with things that aren't
output. A command reaction is a fire-and-forget notification a handler
raises; any number of independent handlers can react to it.

## How to do it

Declare the reaction, raise it from a command handler with
`ByReacting`, and write one handler per thing that should happen when
it fires:

```csharp
// The reaction — a marker, like a command.
public record OrderPlacedCliCommandReaction(string OrderId) : CliCommandReaction;

// The command handler raises it — it doesn't know or care who's listening.
public class PlaceOrderCliCommandHandler : CliCommandHandler<PlaceOrderCliCommand>
{
    public override Task<Outcome[]> HandleCommand(PlaceOrderCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByReacting(new OrderPlacedCliCommandReaction(command.OrderId))
            .ByFinallySaying($"Order {command.OrderId} placed.")
            .EndAsync();
}

// Any number of independent handlers can react.
public class LogOrderPlacedReactionHandler : CliCommandReactionHandler<OrderPlacedCliCommandReaction>
{
    public override Task HandleReaction(OrderPlacedCliCommandReaction reaction, CancellationToken ct)
    {
        Console.WriteLine($"Order {reaction.OrderId} logged.");
        return Task.CompletedTask;
    }
}

public class NotifyOrderPlacedReactionHandler : CliCommandReactionHandler<OrderPlacedCliCommandReaction>
{
    public override Task HandleReaction(OrderPlacedCliCommandReaction reaction, CancellationToken ct)
    {
        Console.WriteLine($"Order {reaction.OrderId} notification sent.");
        return Task.CompletedTask;
    }
}
```

Both reaction handlers run whenever `PlaceOrderCliCommandHandler`
raises `OrderPlacedCliCommandReaction` — you don't register them
anywhere by hand, and you don't need to know how many reaction
handlers exist (zero, one, or several) when you write the command
handler that raises the reaction.

## Common mistakes

**Using a reaction to produce something the user should see.** A
reaction is a side effect, not output — it never reaches an
`IOutcomeIoWriter`. If it's part of the response to the user's ask,
that's an `Outcome` (`BySaying`, `ByShowingTable`, ...), not a
reaction.

**Using a reaction to pass state to a later command.** A reaction
handler's return value is discarded (`Task`, not `Task<Outcome[]>`) —
there's no path from a reaction handler back into the workflow run. If
a later command needs to read something, that's a `Reusable` outcome
and an artefact — see
[reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md).

**Assuming reaction handlers run in a guaranteed order, or that one
failing stops the others.** Nothing in the reaction mechanism
promises ordering or isolation between multiple handlers for the same
reaction — don't design around either.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the pattern to use instead, when a later command actually needs to
  read state.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — how
  `ByReacting` fits into the full outcome model (a reaction outcome is
  `Anonymous`-kind — it doesn't affect the run's state machine at all).
- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  where reactions actually get published (`ExecuteCommand`, via
  `IPublisher`) relative to the rest of a run's outcome handling.
