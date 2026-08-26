# 0008. Running side effects with command reactions

## What this is for

A command sometimes triggers something the user never sees as the result:
an audit log, a cache invalidation, a notification elsewhere in the app.
Those have no place in the command's outcome list, and cramming every
interested party into the handler clutters it. A reaction is a notification
a handler raises without knowing who listens.

## How to do it

Declare the reaction, raise it with `ByReacting`, and write one handler per
thing that should happen:

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
```

Add a second handler for the same reaction and both run. You register
neither by hand, and the command handler needs no idea how many exist —
zero, one, or several.

## Common mistakes

**Using a reaction to produce something the user should see.** A reaction
never reaches an `IOutcomeIoWriter`. Anything forming part of the response
to the ask is an `Outcome`: `BySaying`, `ByShowingTable`, and the rest.

**Using a reaction to pass state to a later command.** A reaction handler
returns `Task`, not `Task<Outcome[]>`, and its result is discarded. When a
later command must read something, use a `Reusable` outcome and an artefact
— see
[0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md).

**Assuming a reaction cannot break the command that raised it.** It can.
Reaction handlers are awaited before the command's outcomes return, inside
the same try block, so one that throws takes the whole run to
`Exceptional`. Catch inside your reaction handler when its failure should
leave the command alone.

**Relying on the order reaction handlers run in, or on one failing leaving
the others alone.** Neither is guaranteed. Design around neither.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  the pattern to use when a later command must read state.
- [../concepts/0006-outcomes.md](../concepts/0006-outcomes.md) — how
  `ByReacting` fits the outcome model. A `ReactionOutcome` is
  `Anonymous`-kind and leaves the run's state machine alone.
- [../concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  what an exception during a reaction does to the run.
