# 0006. Gating a command with CanCreateWhen

## What this is for

Some commands only make sense sometimes: after a particular earlier
command, or for one sub-command word. `CanCreateWhen()` on the factory
holds that condition, so an ask that shouldn't work never reaches a
handler.

## How to do it

### Gating on a sub-command word

`BasicDecisionCliCommandFactory<T>` writes `Create()` for you, leaving
`CanCreateWhen` as the only thing to fill in:

```csharp
public record StartCliCommand : CliCommand;

public class StartCliCommandFactory : BasicDecisionCliCommandFactory<StartCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("test");
}
```

`/start test` resolves to `StartCliCommand`. `/start`, and `/start
anything-else`, fall through to "no matching command", exactly as though no
factory existed.

### Gating on what already happened in this run

Any `CliCommandFactory<T>` can call `LastCommandWas<TPriorCommand>()` or
query an artefact, offering a command only once an earlier step has
happened:

```csharp
public class ConfirmDeleteCliCommandFactory : CliCommandFactory<ConfirmDeleteCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<RequestDeleteCliCommand>();

    public override CliCommand Create() => new ConfirmDeleteCliCommand();
}
```

Typing `/confirm-delete` before `/request-delete` fails to resolve, so
nobody reaches the confirmation step out of order. This needs
`AddArtefactFactoriesForAssembly` in your registry — without it
`LastCommandWas` always returns `false`. See
[0004-creating-a-registry.md](0004-creating-a-registry.md).

Despite its name, `LastCommandWas<T>()` is `true` when `T` ran at any point
in the current run, not only most recently. Use it to ask "has this step
happened yet", never "is this the step immediately before".

## Common mistakes

**Putting the check inside the handler.** A handler that runs and then asks
"was this valid to call?" asks one step too late. Move eligibility into
`CanCreateWhen`, where an invalid ask fails to resolve before any handler
logic runs.

**Expecting a `false` to give the user a helpful error.** The ask fails to
resolve to any command, like a typo would, and no "almost matched but
rejected" message exists. To tell the user why, declare the valid moves on
the *previous* command with `[CliNextCommandIs(name, description)]`
([0014-suggesting-what-to-run-next.md](0014-suggesting-what-to-run-next.md)).

**Writing a second factory for the same command type to route between two
behaviours.** `AddCommandsFromAssembly` throws at startup ("Multiple
factories found for command type"). `CanCreateWhen` gates whether *one*
factory's command is offered. For genuinely different behaviour, branch
inside one `Create()`, or give each variant its own command type.

## Learn more

- [0001-writing-a-basic-command.md](0001-writing-a-basic-command.md) — when you
  need a factory at all.
- [0005-reading-command-arguments.md](0005-reading-command-arguments.md) — the
  argument helpers `CanCreateWhen` can also read.
- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  where `CanCreateWhen` sits in resolution: keyed DI narrows the
  candidates, `CanCreateWhen` picks among them, first match wins.
