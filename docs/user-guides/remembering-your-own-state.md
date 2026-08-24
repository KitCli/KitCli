# Remembering your own state

## What this is for

[Remembering state across asks](reusable-outcomes-and-the-workflow-run.md)
covers the built-in `Reusable` outcomes: page size, filters, and the rest.
Sooner or later you need something of your own — a selected account, a
chosen budget ID, anything a later command in the same run should read
back. This is the three-type recipe.

## How to do it

**1. A `Reusable` outcome**, what your handler returns. `Reusable` keeps
the run going and available for the next ask, the right kind for state a
later command reads:

```csharp
public record SelectedAccountOutcome(string AccountId) : Outcome(OutcomeKind.Reusable);
```

**2. An artefact**, the queryable form a later factory looks up:

```csharp
public record SelectedAccountArtefact(string AccountId)
    : Artefact<string>(nameof(SelectedAccountArtefact), AccountId);
```

**3. A factory converting one into the other:**

```csharp
public class SelectedAccountArtefactFactory : ArtefactFactory<SelectedAccountOutcome>
{
    protected override AnonymousArtefact CreateArtefact(SelectedAccountOutcome outcome)
        => new SelectedAccountArtefact(outcome.AccountId);
}
```

Raise the outcome with `ByResultingIn`. Custom outcomes have no `By...`
shortcut of their own, as the built-ins do:

```csharp
public class SelectAccountCliCommandHandler : CliCommandHandler<SelectAccountCliCommand>
{
    public override Task<Outcome[]> HandleCommand(SelectAccountCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying($"Selected account {command.AccountId}.")
            .ByResultingIn(new SelectedAccountOutcome(command.AccountId))
            .EndAsync();
}
```

Keep the reusable outcome last. Only the final outcome decides what the
run does next, so ending on `ByFinallySaying` would finish the run and
discard the account just selected.

Read it back in a later command's factory, like any built-in artefact:

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

**Discovery is automatic; the call that starts it is not.** Your registry
must call `AddArtefactFactoriesForAssembly(assembly)`, separate from
`AddCommandsFromAssembly`, which leaves artefact factories alone. Given
that one call, every `ArtefactFactory<>` subclass in the assembly is found
and registered alongside the built-ins, and you name
`SelectedAccountArtefactFactory` nowhere. See
[creating-a-registry.md](creating-a-registry.md).

## Common mistakes

**Forgetting `AddArtefactFactoriesForAssembly` in the registry.** All
three types compile, and the outcome returns happily. The failure appears
when `GetRequiredArtefact` throws at runtime in the later command.

**Skipping the artefact and factory to re-parse the outcome history
yourself.** Reading the run's raw history bypasses the type and name
lookup (`GetArtefact`, `GetRequiredArtefact`) every other piece of
remembered state uses. Write the three types, however small the value.

**Making an outcome `Reusable` when nothing reads it back.** With no later
factory querying the value, it needs no artefact pair. An `Anonymous`
outcome — `BySaying`, or your own `Outcome(OutcomeKind.Anonymous)` —
suffices for something purely informational.

**Giving two custom artefacts the same `Name`, or none when more than one
could exist.** `GetArtefact<T>` filters by type, then by name if given,
and takes the *last* match. An unnamed or colliding lookup silently
returns whichever was set most recently.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the built-in `Reusable` outcomes this pattern generalizes.
- [reading-command-arguments.md](reading-command-arguments.md) —
  `GetArgument` and `GetRequiredArgument`, for the current ask's arguments
  rather than a prior command's state.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — the mechanics
  beneath: registration, "last match wins" lookup, and where a factory's
  artefact list comes from at runtime.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — the full
  `Outcome` and `OutcomeKind` model a custom outcome plugs into.
