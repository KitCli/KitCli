# 0009. Remembering your own state

## What this is for

[Remembering state across asks](0010-reusable-outcomes-and-the-workflow-run.md)
covers the built-in `Reusable` outcomes — page size, filters, and the rest.
Sooner or later you need something of your own: a selected account, a
chosen budget ID, anything a later command in the same run should read
back. This is the three-type recipe.

## How to do it

**1. A `Reusable` outcome**, what your handler returns. `Reusable` keeps
the run going and its data available for the next ask:

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
return FinishThisCommand()
    .BySaying($"Selected account {command.AccountId}.")
    .ByResultingIn(new SelectedAccountOutcome(command.AccountId))
    .EndAsync();
```

Keep the reusable outcome last. Only the final outcome decides what the run
does next, so ending on `ByFinallySaying` would finish the run and discard
the account you just selected.

Read it back in a later command's factory, like any built-in artefact:

```csharp
var accountId = GetRequiredArtefact<string>(nameof(SelectedAccountArtefact)).Value;
```

**Discovery is automatic; the call that starts it is not.** Your registry
must call `AddArtefactFactoriesForAssembly(assembly)`, separate from
`AddCommandsFromAssembly`. Given that one call, every `ArtefactFactory<>`
in the assembly is found, and you name `SelectedAccountArtefactFactory`
nowhere. See [0004-creating-a-registry.md](0004-creating-a-registry.md).

## Common mistakes

**Forgetting `AddArtefactFactoriesForAssembly`.** All three types compile,
and the outcome returns happily. The failure appears when
`GetRequiredArtefact` throws at runtime in the later command.

**Skipping the artefact and factory to re-read the run's history
yourself.** That bypasses the type-and-name lookup every other piece of
remembered state uses. Write the three types, however small the value.

**Making an outcome `Reusable` when nothing reads it back.** With no later
factory querying the value, it needs no artefact pair. An `Anonymous`
outcome suffices for something purely informational.

**Giving two custom artefacts the same `Name`, or none when more than one
could exist.** `GetArtefact<T>` filters by type, then by name if given, and
takes the *last* match. An unnamed or colliding lookup silently returns
whichever was set most recently.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  the built-in `Reusable` outcomes this pattern generalises.
- [../concepts/0008-artefacts.md](../concepts/0008-artefacts.md) — registration,
  "last match wins" lookup, and where a factory's artefact list comes from.
- [../concepts/0006-outcomes.md](../concepts/0006-outcomes.md) — the full
  `Outcome` and `OutcomeKind` model a custom outcome plugs into.
