# Running side effects with command reactions

## What this is for

A command sometimes triggers a side effect the user never sees as the
result: an audit log, a cache invalidation, a notification to elsewhere in
the app. Cramming every interested party into the command handler
clutters it, and those side effects have no place in its outcome list. A
command reaction is a notification a handler raises without knowing who
listens; any number of independent handlers can answer it.

## How to do it

Declare the reaction, raise it from a command handler with `ByReacting`,
and write one handler per thing that should happen:

```csharp
// The reaction — a marker, like a command.
public record OrderPlacedCliCommandReaction(string OrderId) : CliCommandReaction;

// The command handler raises it, knowing nothing about who listens.
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

Both handlers run whenever `PlaceOrderCliCommandHandler` raises
`OrderPlacedCliCommandReaction`. You register neither by hand, and the
command handler needs no idea how many exist — zero, one, or several.

## Common mistakes

**Using a reaction to produce something the user should see.** A reaction
is a side effect and never reaches an `IOutcomeIoWriter` (see
[docs/concepts/outcome-writing.md](../concepts/outcome-writing.md)). Anything forming
part of the response to the ask is an `Outcome`: `BySaying`,
`ByShowingTable`, and the rest.

**Using a reaction to pass state to a later command.** A reaction
handler returns `Task`, not `Task<Outcome[]>`, and its result is
discarded; no path leads from a reaction back into the run. When a later
command must read something, use a `Reusable` outcome and an artefact —
see
[reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md).

**Assuming a reaction can't break the command that raised it.** It can.
Reaction handlers are awaited before the command's outcomes return, inside
the same try block, so one that throws takes the whole run to
`Exceptional`. Catch inside your reaction handler when its failure should
leave the command alone.

**Assuming reaction handlers run in a guaranteed order, or that one
failing stops the others.** The mechanism promises neither ordering nor
isolation between handlers for the same reaction. Design around neither.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the pattern to use when a later command must read state.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — how `ByReacting`
  fits the outcome model. A reaction outcome is `Anonymous`-kind and
  leaves the run's state machine alone.
- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  where reactions are published (`ExecuteCommand`, via `IPublisher`), and
  what an exception there does to the run.
