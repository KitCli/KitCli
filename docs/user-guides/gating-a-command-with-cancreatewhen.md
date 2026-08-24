# Gating a command with CanCreateWhen

## What this is for

A command sometimes belongs only under certain conditions: after a
specific prior command, or for one sub-command word. `CanCreateWhen()`
holds that condition, not the handler.

## How to do it

### Gating on a sub-command word

`BasicDecisionCliCommandFactory<T>` implements `Create()` for you, leaving
`CanCreateWhen` alone to write. It pairs naturally with `SubCommandIs`:

```csharp
public record StartCliCommand : CliCommand;

public class StartCliCommandFactory : BasicDecisionCliCommandFactory<StartCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("test");
}
```

`/start test` resolves to `StartCliCommand`. `/start`, and `/start
anything-else`, fall through to "no matching command", exactly as though
no factory existed.

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
nobody reaches the confirmation step out of order.

Despite its name, `LastCommandWas<T>()` returns `true` when `T` ran at any
point in the current run, not only when it ran most recently. Use it to
ask "has this step happened yet", never "is this the step immediately
before".

## Common mistakes

**Putting the check inside the handler instead of `CanCreateWhen`.** A
handler that runs and then asks "was this valid to call?" asks one step
too late; the command already resolved and ran. Move eligibility checks
into `CanCreateWhen`, where an invalid ask fails to resolve before any
handler logic runs.

**Assuming a `false` from `CanCreateWhen` gives the user a helpful
error.** The ask fails to resolve to any command, like a typo or an
unknown name, and no "almost matched but rejected" message exists. To tell
the user why — "you need to select an account first" — declare the valid
moves on the previous command with `[CliNextCommandIs(name, description)]`.
Mid-run, an ask resolving to nothing then prints those suggestions instead
of failing silently.

**Writing a second factory for the same command type to route between two
behaviors.** `AddCommandsFromAssembly` throws at startup ("Multiple
factories found for command type") on finding two `CliCommandFactory<T>`
for one `T` in the scanned assembly. `CanCreateWhen` gates whether *one*
factory's command is offered; it never lets competing factories share a
command type. For genuinely different behavior, branch inside one
`Create()` or handler, or give each variant its own command type.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — when you need
  a factory, versus the automatic one.
- [reading-command-arguments.md](reading-command-arguments.md) and
  [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the argument and artefact helpers available inside `CanCreateWhen` and
  `Create()`, alongside `SubCommandIs` and `LastCommandWas`.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  how `CanCreateWhen` fits into command resolution: keyed DI narrows the
  candidates, `CanCreateWhen` picks among them, first match wins.
