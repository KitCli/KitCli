# Remembering your own state

## What this is for

[Remembering state across asks](reusable-outcomes-and-the-workflow-run.md)
covers the built-in `Reusable` outcomes — page size, filters, and so
on. Sooner or later you'll need to remember something domain-specific
of your own — a selected account, a chosen budget ID, anything a later
command in the same run should be able to read back. This is the
three-type recipe for that.

## How to do it

**1. A `Reusable` outcome** — what your handler returns:

```csharp
public record SelectedAccountOutcome(string AccountId) : Outcome(OutcomeKind.Reusable);
```

**2. An artefact** — the queryable form a later command's factory can
look up:

```csharp
public record SelectedAccountArtefact(string AccountId)
    : Artefact<string>(nameof(SelectedAccountArtefact), AccountId);
```

**3. A factory that converts one into the other:**

```csharp
public class SelectedAccountArtefactFactory : ArtefactFactory<SelectedAccountOutcome>
{
    protected override AnonymousArtefact CreateArtefact(SelectedAccountOutcome outcome)
        => new SelectedAccountArtefact(outcome.AccountId);
}
```

Raise the outcome from a handler with `ByResultingIn` — there's no
`By...` shortcut for a custom outcome the way there is for built-ins:

```csharp
public class SelectAccountCliCommandHandler : CliCommandHandler<SelectAccountCliCommand>
{
    public override Task<Outcome[]> HandleCommand(SelectAccountCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByResultingIn(new SelectedAccountOutcome(command.AccountId))
            .ByFinallySaying($"Selected account {command.AccountId}.")
            .EndAsync();
}
```

Then read it back in a later command's factory, same as any built-in
artefact:

```csharp
public class ShowBalanceCliCommandFactory : CliCommandFactory<ShowBalanceCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var accountId = GetRequiredArtefact<string>(nameof(SelectedAccountArtefact)).Value;
        return new ShowBalanceCliCommand(accountId);
    }
}
```

**Registration is automatic** — you don't call anything to wire the
factory up. `AddCommandsFromAssembly` (called from your registry, see
[creating-a-registry.md](creating-a-registry.md)) scans for every
`ArtefactFactory<>` subclass in the assembly and registers it
alongside the built-in ones.

## Common mistakes

**Skipping the artefact/factory and just re-parsing the outcome from
history yourself.** It's tempting to reach for the run's raw outcome
history directly, but that bypasses the type/name-based lookup
(`GetArtefact`/`GetRequiredArtefact`) every other piece of remembered
state uses — write the three types instead, even for something
small.

**Making an outcome `Reusable` when nothing ever reads it back.** If
no later command's factory needs to query this value, it doesn't need
an artefact pair at all — an `Anonymous` outcome (`BySaying`, or your
own `Outcome(OutcomeKind.Anonymous)`) is enough for something that's
purely informational.

**Giving two different custom artefacts the same `Name` (or none,
when more than one could exist).** `GetArtefact<T>` filters by type
first, then by name if given, taking the *last* match — an unnamed or
colliding-name lookup silently returns whichever was set most
recently, not necessarily the one you meant.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the built-in `Reusable` outcomes this guide's pattern generalizes.
- [reading-command-arguments.md](reading-command-arguments.md) — the
  equivalent `GetArgument`/`GetRequiredArgument` helpers, for the
  current ask's arguments rather than a prior command's remembered
  state.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — the full
  mechanics underneath this guide: automatic registration, "last
  match wins" lookup semantics, and where the artefact list a factory
  reads from actually comes from at runtime.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — the full
  `Outcome`/`OutcomeKind` model a custom outcome plugs into.
