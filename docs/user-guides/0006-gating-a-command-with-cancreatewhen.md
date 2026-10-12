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

Any `CliCommandFactory<T>` can call `LastCommandIs<TPriorCommand>()` or
query an artefact, offering a command only once an earlier step has
happened:

```csharp
public class ConfirmDeleteCliCommandFactory : CliCommandFactory<ConfirmDeleteCliCommand>
{
    public override bool CanCreateWhen() => LastCommandIs<RequestDeleteCliCommand>();

    public override CliCommand Create() => new ConfirmDeleteCliCommand();
}
```

Typing `/confirm-delete` before `/request-delete` fails to resolve, so
nobody reaches the confirmation step out of order. This needs
`AddArtefactFactoriesForAssembly` in your registry — without it
`LastCommandIs` always returns `false`. See
[0004-creating-a-registry.md](0004-creating-a-registry.md).

`LastCommandIs<T>()` is `true` only when `T` is the step immediately before,
and `false` once another command has run since. To accept a repeat of the
same step, such as turning a page twice, check for both:
`LastCommandIs<List>() || LastCommandIs<NextPage>()`. The older
`LastCommandWas<T>()` is obsolete: despite its name it is `true` when `T` ran
at any earlier point in the run.

## Common mistakes

**Putting the check inside the handler.** A handler that runs and then asks
"was this valid to call?" asks one step too late. Move eligibility into
`CanCreateWhen`, where an invalid ask fails to resolve before any handler
logic runs.

**Expecting a `false` to give the user a helpful error.** The ask fails to
resolve to any command, like a typo would, and no "almost matched but
rejected" message exists. To tell the user why, declare the valid moves on
the *previous* command with `[CliNextCommandIs(name, description)]`
([0016-suggesting-what-to-run-next.md](0016-suggesting-what-to-run-next.md)).

## Learn more

- [0001-writing-a-basic-command.md](0001-writing-a-basic-command.md) — when you
  need a factory at all.
- [0005-reading-command-arguments.md](0005-reading-command-arguments.md) — the
  argument helpers `CanCreateWhen` can also read.
- [../concepts/command-registration.md](../concepts/command-registration.md) —
  where `CanCreateWhen` sits in resolution: keyed DI narrows the
  candidates, `CanCreateWhen` picks among them, first match wins.
- [0017-creating-sub-commands.md](0017-creating-sub-commands.md) —
  writing several `CliCommandFactory<T>` for one command type, one per
  sub-command word.
