# 0009. Transport typed payloads in non-generic envelopes, never `dynamic`

Status: Accepted (retroactive — reconstructed from the current source, not
original notes)
Date: 2026-08-24

## Context

KitCli repeatedly has to move a value of an unknown type through machinery
that cannot name that type. A run holds artefacts from every prior command,
each carrying a different value type. A parsed instruction holds arguments
whose types depend on what the user typed. A handler returns outcomes whose
concrete types the workflow engine does not know.

C# offers two ways out. `dynamic` defers member resolution to runtime. An
envelope is a non-generic base type that a generic subtype derives from,
with the concrete type recovered later by a type test.

The choice matters more here than in an application, because KitCli ships
as nine NuGet packages. Whatever it picks constrains every consuming app.

## Decision

KitCli uses non-generic envelopes everywhere and does not use `dynamic` at
all. There are zero occurrences of the keyword in the repository, and zero
anonymous types. Eight envelopes carry the pattern:

| Envelope | Erased from | Recovered by |
|---|---|---|
| `AnonymousArtefact` | `Artefact<TArtefactValue>` | `OfType<>` in `CliCommandFactory` |
| `AnonymousInstructionArgument` | `InstructionArgument<TArgumentValue>` | `OfType<>` in `CliCommandFactory` |
| `Outcome` | generic outcomes; handlers return `Outcome[]` | `OfType<>` in the workflow run |
| `IArtefactFactory` | `ArtefactFactory<TOutcome>` | `is TOutcome` |
| `ICliCommandFactory` | `CliCommandFactory<TCliCommand>` | DI resolution |
| `IOutcomeIoWriter` | never generic | `is` + cast |
| `IInstructionArgumentBuilder` | never generic | `For(string?)` probe |
| `ICliWorkflowRunStateChange` | `IOutcomeCliWorkflowRunStateChange` | `OfType<>` |

**`Anonymous` in a type name means type-erased.** It does not mean the same
thing as `OutcomeKind.Anonymous`, which means an outcome has no effect on
the run. The two are unrelated.

## Alternatives considered

- **`dynamic` at the dispatch points** — would collapse the hand-rolled
  double dispatch in the eight writers into one call site. It loses on
  three counts. It removes the compiler as the contract, and for a
  conventions framework the compile-time contract *is* the product. It is
  not trim- or AOT-compatible: the binder is
  `[RequiresUnreferencedCode]`, so one use emits IL2026 into every consumer
  publishing trimmed or Native AOT. And it costs more per dispatch than a
  type test.
- **The visitor pattern** — forces a cyclic dependency: every new outcome
  type means touching a visitor interface every writer implements.
  Envelopes plus a type test leave both sides independent.

## Consequences

- Every public extension point is a compile-time contract. Getting one
  wrong is a compiler error, not a `RuntimeBinderException` in front of a
  user.
- Recovering the type becomes the framework's own work, across roughly 31
  sites. Each is a place a new subtype can be silently missed rather than
  failing to compile.
- The eight writers are the weakest of those. `ArtefactFactory.Create`
  recovers its type with `is TOutcome` and throws a named exception on a
  mismatch; the writers cast unchecked after a separate `CanWriteFor`, so a
  misregistered writer throws `InvalidCastException`. Tracked as
  [#117](https://github.com/KitCli/KitCli/issues/117).
- **This does not make KitCli AOT-safe.** The five `MakeGenericType` and
  `Activator.CreateInstance` sites from
  [ADR 0003](0003-reflection-based-automatic-registration.md) are
  trim-hostile for the same reason `dynamic` is. No project sets
  `IsTrimmable` or `IsAotCompatible`, so the warnings are unmeasured.

## References

- [dotnet/linker#1156](https://github.com/dotnet/linker/issues/1156) —
  "The `dynamic` contextual keyword in C# is not friendly for trimming."
- [Prepare .NET libraries for trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)
- [Exploring Multiple Dispatch in C# with BenchmarkDotNet](http://techblog.jetabroad.com/2017/05/exploring-multiple-dispatch-in-c-with.html)
