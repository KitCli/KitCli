# Artefacts

## Premise

A later command in the same run often needs something an earlier
command produced — a page size set two commands ago, a filter chosen
earlier, an aggregator a list command built. `Artefact` is the reusable
form of that: the same data, made queryable by type and name across the
whole run.

## Problem

An [outcome](outcomes.md) is scoped to the command that returned it —
nothing about it is addressable by a later command's factory. Something
has to convert "what just happened" into "what's queryable, by type and
name, across the whole run," without forcing every outcome to also be a
well-typed, name-addressable value — even purely informational ones
like `SayOutcome`, which have no reason to be looked up later.

## Solution

### Creating a custom outcome, with its artefact factory

```csharp
// 1. The outcome — a command handler returns this.
public record ThresholdOutcome(int Threshold) : Outcome(OutcomeKind.Reusable);

// 2. The artefact — what later commands can query for.
public record ThresholdArtefact(int Threshold)
    : Artefact<int>(nameof(Threshold), Threshold);

// 3. The factory — converts one outcome into its artefact.
public class ThresholdArtefactFactory : ArtefactFactory<ThresholdOutcome>
{
    protected override AnonymousArtefact CreateArtefact(ThresholdOutcome outcome)
        => new ThresholdArtefact(outcome.Threshold);
}
```

`ArtefactFactory<TOutcome>` (`Artefacts/ArtefactFactory.cs`) implements
`IArtefactFactory.For(Outcome)` as `outcome is TOutcome` for you — you
only implement `CreateArtefact`. Every `Artefact<TValue>` derives from
`AnonymousArtefact(string Name)` and adds a typed `Value`.

**Registration is automatic** — you don't call anything.
`AddArtefactFactoriesForAssembly`
(`Extensions/ArtefactServiceCollectionExtensions.cs`) reflection-scans
the consumer's assembly for every class extending `ArtefactFactory<>`
and registers it, alongside built-in ones (page size/number, the
ran-command marker, aggregators, table builders — anything generic gets
its closed generic factory built via `MakeGenericType` +
`Activator.CreateInstance`). Write the three types above;
registration is free.

### Using artefacts in a later command

`CliCommandFactory<TCliCommand>` (`Factories/CliCommandFactory.cs`) is
the base class for command factories that need to inspect prior state.
It exposes:

```csharp
protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? name = null);
protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? name = null);   // throws if missing
protected bool AnyArtefact<TArtefactType>(string? name);
protected bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand;
```

(Argument equivalents — `GetArgument<T>`, `GetRequiredArgument<T>`,
`AnyArgument<T>` — work the same way over the current instruction's
parsed arguments, not artefacts; see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md).)
All of these filter `Artefacts` (a `List<AnonymousArtefact>` the
workflow engine attaches before your factory runs, via
`Attach(instruction, artefacts)`) by type — and, if given, by `Name` —
taking the **last** match. That "last wins" semantics matters: if a
threshold was set twice in a run, later commands see the most recent
one.

```csharp
public class FilteredListCommandFactory : CliCommandFactory<FilteredListCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var threshold = GetRequiredArtefact<int>(nameof(ThresholdArtefact.Threshold));
        return new FilteredListCommand(threshold.Value);
    }
}
```

### The whole loop

```
Command A's handler
  → returns Outcome[]  (see outcomes.md)
      → CliWorkflowRun records it in state history
          → an ArtefactFactory<TOutcome> converts each Reusable outcome into an Artefact
              → Command B's CliCommandFactory<T> queries artefacts by type/name
                  → Command B is constructed with that data
```

## Constraints & tradeoffs

**Two-stage model (Outcome → Artefact) instead of passing state
directly.** An outcome is "what just happened, for this command's own
consumers" (the IO writer, the workflow state machine); an artefact is
"what's queryable by name/type across the whole run." Collapsing these
into one type would force every outcome to also be a well-typed,
name-addressable artefact, even outcomes that are purely informational.

**Reflection-based automatic registration over manual DI wiring.**
Saves the boilerplate of registering every factory by hand, at the cost
of relying on `Activator.CreateInstance` and assembly scanning —
factories must have a parameterless constructor, and a factory that
isn't in the scanned assembly silently isn't registered (no error at
startup).

**"Last match wins" for `GetArtefact`/`GetArgument`, not "all matches."**
Simple and matches the common case (most recent value for a repeated
setting), but a factory that genuinely needs the *history* of a value
(not just its latest) has to go around this API and inspect
`State.AllOutcomeStateChanges()` directly.

## Questions & answers

**When does an outcome need an artefact/factory pair, versus just being an outcome?**
Only when a *later command's factory* needs to look it up. A one-off
message or table that nothing else references doesn't need an
artefact — `SayOutcome`/`TableOutcome` have no artefact factories today.

**Why does `GetRequiredArtefact` throw a bare `Exception` instead of something typed?**
It does today — that's a known gap, not intentional design. Don't take
the current exception type as a contract to catch against.

**What happens if two artefact factories can produce the same artefact type?**
`GetArtefact<T>` takes the *last* matching artefact in the list —
whichever outcome produced it most recently in the run's history, not
whichever factory registered "first."

**Where does the artefact list actually come from at runtime?**
`CliWorkflowCommandProvider` builds it from the run's outcome history
before attaching it to a factory — a command factory never constructs
this list itself, only reads from it via the `CliCommandFactory<T>`
base class helpers.

## Related concepts

- [outcomes.md](outcomes.md) — what a command handler actually returns;
  every artefact starts life as a `Reusable` outcome.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) —
  `CliWorkflowCommandProvider` builds the artefact list from the run's
  outcome history before a factory runs.
- [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md) —
  `GetArgument<T>`/`GetRequiredArgument<T>` work the same way as the
  artefact helpers, over the current instruction's parsed arguments
  instead.
- [command-registration.md](command-registration.md) — how the
  `CliCommandFactory<T>` reading these artefacts is itself resolved.
- [0003-reflection-based-automatic-registration.md](../adr/0003-reflection-based-automatic-registration.md) —
  why artefact factories are discovered by assembly scan rather than
  registered by hand.
