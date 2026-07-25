# Outcome → Artefact pipeline

## Premise

A KitCli command handler returns the result of running a command. But
commands often need to build on what a *prior* command did — a page
number set two commands ago, a filter chosen earlier, an aggregator
constructed by an earlier list command. KitCli splits "what a command
produced" into two layers to make that reuse possible without every
handler manually threading state through: **Outcomes** (what a command
communicates happened) and **Artefacts** (the reusable form of that,
available to later commands in the same run).

## Problem

A single command handler needs to do three distinct things, and
conflating them gets messy fast:

1. Tell the workflow engine what kind of result this was — does the run
   continue, pause for more input, or end? (`Outcome`)
2. Tell the output layer what to show the user — a message, a table, an
   error. (also `Outcome`, via `IOutcomeIoWriter`)
3. Make some piece of that result available to a *later* command in the
   same run, in a typed, queryable way. (`Artefact`)

Outcomes alone can't cleanly do (3) — an `Outcome` is the record of what
just happened, scoped to one command's return value. A later command
factory needs to search *all* outcomes since the run started for the last
page size, the last aggregator, etc. Artefacts are the derived, queryable
form of outcome history that makes that search possible.

## Solution

### Part 1 — returning outcomes from a command handler

Every `CliCommand` handler returns `Outcome[]`, built with the
`OutcomeList` fluent builder (`KitCli.Commands.Abstractions/Outcomes/OutcomeList.cs`):

```csharp
public class MyCommandHandler : CliCommandHandler<MyCommand>
{
    public override Task<Outcome[]> HandleCommand(MyCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByRememberingPageSize(20)
            .ByShowingTable(table)
            .ByFinallySaying("Done.")
            .EndAsync();
}
```

`FinishThisCommand()` just returns `[]` (an empty `OutcomeList`, itself a
`List<Outcome>`) — every `By...` method appends one more `Outcome` and
returns `this`, so calls chain. `.End()` / `.EndAsync()` materialize the
list to `Outcome[]` / `Task<Outcome[]>`.

Every `Outcome` carries an `OutcomeKind` (`Outcome.cs`):

| Kind | Meaning |
|---|---|
| `Anonymous` | Has no effect on the workflow run — e.g. a plain message. |
| `Reusable` | Allows further operation on the same run — the run continues. |
| `Final` | Ends the workflow run. |

`Outcome.IsReusable` is just `Kind == OutcomeKind.Reusable`; the workflow
engine (`CliWorkflowRun.UpdateStateAfterOutcome`) looks at the **last**
outcome in the returned array to decide whether the run continues, moves
to `ReachedReusableOutcome`, or ends at `ReachedFinalOutcome`.

### Part 2 — creating a custom outcome, with its artefact factory

To make an outcome's data available to later commands, give it an
`Artefact` and a factory that converts one to the other:

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

**Registration is automatic** — you don't call anything. `AddArtefactFactoriesForAssembly`
(`Extensions/ArtefactServiceCollectionExtensions.cs`) reflection-scans the
consumer's assembly for every class extending `ArtefactFactory<>` and
registers it, alongside built-in ones (page size/number, the ran-command
marker, aggregators, table builders — anything generic gets its closed
generic factory built via `MakeGenericType` + `Activator.CreateInstance`).
Write the three types above; registration is free.

### Part 3 — using artefacts in a later command

`CliCommandFactory<TCliCommand>` (`Factories/CliCommandFactory.cs`) is the
base class for command factories that need to inspect prior state. It
exposes:

```csharp
protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? name = null);
protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? name = null);   // throws if missing
protected bool AnyArtefact<TArtefactType>(string? name);
protected bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand;
```

(Argument equivalents — `GetArgument<T>`, `GetRequiredArgument<T>`,
`AnyArgument<T>` — work the same way over the current instruction's
parsed arguments, not artefacts.) All of these filter `Artefacts` (a
`List<AnonymousArtefact>` the workflow engine attaches before your
factory runs, via `Attach(instruction, artefacts)`) by type — and, if
given, by `Name` — taking the **last** match. That "last wins" semantics
matters: if a threshold was set twice in a run, later commands see the
most recent one.

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
  → returns Outcome[]  (via OutcomeList)
      → CliWorkflowRun records it in state history
          → an ArtefactFactory<TOutcome> converts each outcome into an Artefact
              → Command B's CliCommandFactory<T> queries artefacts by type/name
                  → Command B is constructed with that data
```

## Constraints & tradeoffs

**Two-stage model (Outcome → Artefact) instead of passing state directly.**
An outcome is "what just happened, for this command's own consumers" (the
IO writer, the workflow state machine); an artefact is "what's queryable
by name/type across the whole run." Collapsing these into one type would
force every outcome to also be a well-typed, name-addressable artefact,
even outcomes that are purely informational (`SayOutcome`, `NothingOutcome`)
and have no reason to be queried later.

**Reflection-based automatic registration over manual DI wiring.** Saves
the boilerplate of registering every factory by hand, at the cost of
relying on `Activator.CreateInstance` and assembly scanning — factories
must have a parameterless constructor, and a factory that isn't in the
scanned assembly silently isn't registered (no error at startup).

**"Last match wins" for `GetArtefact`/`GetArgument`, not "all matches."**
Simple and matches the common case (most recent value for a repeated
setting), but a factory that genuinely needs the *history* of a value
(not just its latest) has to go around this API and inspect
`State.AllOutcomeStateChanges()` directly.

## Questions & answers

**When does an outcome need an artefact/factory pair, versus just being an outcome?**
Only when a *later command's factory* needs to look it up. A one-off
message or table that nothing else references doesn't need an artefact —
`SayOutcome`/`TableOutcome` have no artefact factories today.

**Why does `GetRequiredArtefact` throw a bare `Exception` instead of something typed?**
It does today — that's a known gap, not intentional design (see the
architectural review's findings on `CliCommandFactory`'s exception
handling). Don't take the current exception type as a contract to catch
against.

**What happens if two artefact factories can produce the same artefact type?**
`GetArtefact<T>` takes the *last* matching artefact in the list —
whichever outcome produced it most recently in the run's history, not
whichever factory registered "first."

**Where does the artefact list actually come from at runtime?**
`CliWorkflowCommandProvider` builds it from the run's outcome history
before attaching it to a factory — a command factory never constructs
this list itself, only reads from it via the `CliCommandFactory<T>` base
class helpers.
