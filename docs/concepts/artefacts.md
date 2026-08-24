# Artefacts

## Premise

A later command in the same run often needs something an earlier command
produced: a page size set two commands ago, a filter chosen earlier, an
aggregator a list command built. `Artefact` is that data made queryable
by type and name across the whole run.

## Problem

An [outcome](outcomes.md) belongs to the command that returned it, and no
later factory can address it. Something must convert "what just happened"
into "what's queryable across the run" — without forcing every outcome to
become a well-typed, name-addressable value. Purely informational ones
like `SayOutcome` have no reason to be looked up later.

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
`IArtefactFactory.For(Outcome)` as `outcome is TOutcome`, leaving you only
`CreateArtefact`. Every `Artefact<TValue>` derives from
`AnonymousArtefact(string Name)` and adds a typed `Value`.

**Discovery is automatic; the call that starts it is not.** A registry
calls `AddArtefactFactoriesForAssembly(assembly)`
(`Extensions/ArtefactServiceCollectionExtensions.cs`) once per assembly.
This is a *separate* call from `AddCommandsFromAssembly`, which registers
commands, factories, and handlers but no artefact factories.

That one call scans the assembly and registers:

- every class extending `ArtefactFactory<>`, like `ThresholdArtefactFactory`
  above,
- an `AggregatorArtefactFactory<,>` for every closed `Aggregator<,>`,
- a `TableBuilderArtefactFactory<,>` for every closed `TableBuilder<,>`.

The generic ones get their closed type from `MakeGenericType` and
`Activator.CreateInstance`. Four built-in factories register no matter what the
assembly holds: page size, page number, the ran-command marker, and
aggregator filters. Write the three types above and registration follows.

### Using artefacts in a later command

`CliCommandFactory<TCliCommand>` (`Factories/CliCommandFactory.cs`) is the
base class for command factories that inspect prior state. It exposes:

```csharp
protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? name = null);
protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? name = null);   // throws if missing
protected bool AnyArtefact<TArtefactType>(string? name);
protected bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand;
```

The argument equivalents — `GetArgument<T>`, `GetRequiredArgument<T>`,
`AnyArgument<T>` — work the same way over the current instruction's parsed
arguments; see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md).

All of them filter `Artefacts` by type, then by `Name` if you give one,
and take the **last** match. `Artefacts` is a `List<AnonymousArtefact>`
the workflow engine attaches through `Attach(instruction, artefacts)`
before your factory runs. "Last wins" matters: set a threshold twice in a
run, and later commands see the second one.

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
          → an ArtefactFactory<TOutcome> converts every outcome it claims into an Artefact
              → Command B's CliCommandFactory<T> queries artefacts by type/name
                  → Command B is constructed with that data
```

`CliWorkflowCommandProvider` reruns that conversion over the run's whole
outcome history on every command resolution. `OutcomeKind` plays no part:
an outcome becomes an artefact when some registered factory's `For()`
claims its type, and not otherwise.

## Constraints & tradeoffs

**Two stages, outcome then artefact, rather than passing state directly.**
An outcome is what just happened, for this command's own consumers: the IO
writer and the state machine. An artefact is what stays queryable by name
and type across the run. Collapsing the two would force every outcome,
informational ones included, to become a name-addressable artefact.

**Reflection-based registration over manual DI wiring.** This saves
registering every factory by hand, at the cost of `Activator.CreateInstance`
and assembly scanning. Factories need a parameterless constructor. A
factory registers silently — no startup error — when it sits outside a
scanned assembly, or when the registry never calls
`AddArtefactFactoriesForAssembly`. Either way, the first symptom is
`GetRequiredArtefact` throwing at runtime.

**"Last match wins" for `GetArtefact` and `GetArgument`, not "all
matches."** This is simple and fits the common case, the most recent value
of a repeated setting. A factory needing a value's whole *history* must
bypass this API and read `State.AllOutcomeStateChanges()` directly.

## Questions & answers

**When does an outcome need an artefact and factory, rather than being an outcome alone?**
Only when a *later command's factory* looks it up. A one-off message or
table that nothing references needs no artefact; `SayOutcome` and
`TableOutcome` have no artefact factories today.

**Why does `GetRequiredArtefact` throw a bare `Exception` instead of something typed?**
It does today, as does `GetRequiredArgument` — a gap rather than a
decision, given the typed `CliException` hierarchy already used elsewhere.
Tracked as [#34](https://github.com/KitCli/KitCli/issues/34). Treat the
current exception type as temporary, not a contract to catch against.

**What happens if two artefact factories produce the same artefact type?**
`GetArtefact<T>` takes the *last* matching artefact in the list: whichever
outcome produced it most recently, regardless of which factory registered
first.

**Where does the artefact list come from at runtime?**
`CliWorkflowCommandProvider` builds it from the run's outcome history and
attaches it to the factory. A command factory only reads that list,
through the `CliCommandFactory<T>` helpers.

## Related concepts

- [outcomes.md](outcomes.md) — what a command handler returns. Every
  artefact starts as an outcome of any kind; `Reusable` is the usual
  choice, not a requirement.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) —
  `CliWorkflowCommandProvider` builds the artefact list from the run's
  outcome history before a factory runs.
- [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md) —
  `GetArgument<T>` and `GetRequiredArgument<T>` work like the artefact
  helpers, over the current instruction's parsed arguments.
- [command-registration.md](command-registration.md) — how the
  `CliCommandFactory<T>` reading these artefacts is itself resolved.
- [0003-reflection-based-automatic-registration.md](../adr/0003-reflection-based-automatic-registration.md) —
  why an assembly scan discovers artefact factories instead of hand
  registration.
- [0004-first-match-wins-resolution.md](../adr/0004-first-match-wins-resolution.md) —
  why `GetArtefact`'s "last outcome wins" and factory resolution's "first
  factory wins" are instances of one pattern.
