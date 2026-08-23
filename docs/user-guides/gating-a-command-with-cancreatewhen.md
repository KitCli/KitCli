# Gating a command with CanCreateWhen

## What this is for

Sometimes a command should only be offered under certain
conditions — only after a specific prior command ran, or only for a
particular sub-command word. `CanCreateWhen()` is where that
condition belongs, not inside the handler.

## How to do it

### Gating on a sub-command word

`BasicDecisionCliCommandFactory<T>` (a `CliCommandFactory<T>` whose
`Create()` is already implemented, so you only write `CanCreateWhen`)
pairs naturally with `SubCommandIs`:

```csharp
public record StartCliCommand : CliCommand;

public class StartCliCommandFactory : BasicDecisionCliCommandFactory<StartCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("test");
}
```

`/start test` resolves to `StartCliCommand`; `/start` (or `/start
anything-else`) doesn't — it falls through to "no matching command"
instead, the same as if no factory existed at all.

### Gating on what already happened in this run

Any `CliCommandFactory<T>` can check `LastCommandWas<TPriorCommand>()`
or query an artefact directly, to make a command only available once
some earlier step has actually happened:

```csharp
public class ConfirmDeleteCliCommandFactory : CliCommandFactory<ConfirmDeleteCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<RequestDeleteCliCommand>();

    public override CliCommand Create() => new ConfirmDeleteCliCommand();
}
```

Typing `/confirm-delete` before ever running `/request-delete` fails
to resolve — there's no way to reach the confirmation step out of
order.

## Common mistakes

**Putting the same check inside the handler instead of
`CanCreateWhen`.** If a handler runs and then checks "wait, was this
actually valid to call?", that check happened one step too late — the
command already resolved and ran. Move eligibility checks into
`CanCreateWhen` so an invalid ask fails to resolve at all, before any
handler logic runs.

**Assuming a `false` from `CanCreateWhen` gives the user a helpful
error.** It doesn't — the ask just fails to resolve to any command,
the same as a typo or an unrecognized instruction name. If the user
needs to know *why* (e.g. "you need to select an account first"),
that's on you to communicate some other way — there's no built-in
"almost matched but rejected" message.

**Writing a second factory for the same command type to try to
route between two behaviors.** `AddCommandsFromAssembly` throws at
startup ("Multiple factories found for command type") if it finds
more than one `CliCommandFactory<T>` for the same `T` in the scanned
assembly — `CanCreateWhen` gates whether *one* factory's command is
offered, it doesn't let you register competing factories for the same
command type. If you need genuinely different behavior for different
inputs, either branch inside one `Create()`/handler, or give each
variant its own distinct command type.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — when you
  need a factory at all, versus the automatic one.
- [reading-command-arguments.md](reading-command-arguments.md) and
  [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the argument/artefact helpers available inside `CanCreateWhen` and
  `Create()` alongside `SubCommandIs`/`LastCommandWas`.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  exactly how `CanCreateWhen` fits into command resolution: keyed DI
  narrows candidates first, `CanCreateWhen` picks among them second,
  first match wins.
