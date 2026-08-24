# 0009. Transport typed payloads in non-generic envelopes, never `dynamic`

Status: Accepted (retroactive — reconstructed from the current source, not
original notes)
Date: 2026-08-24

## Context

KitCli repeatedly needs to move a value of an unknown type through a piece
of framework machinery that cannot name that type.

The workflow run holds artefacts from every command that ran before it,
each carrying a different value type. A parsed instruction holds arguments
whose types depend on what the user typed. A command handler returns
outcomes whose concrete types the workflow engine doesn't know. In each
case something has to hold "a value plus enough metadata to find it again"
without being generic over that value.

C# offers two ways out of this. One is `dynamic`, which defers member
resolution to runtime. The other is an envelope: a non-generic base type or
interface that a generic subtype derives from, with the concrete type
recovered later by a type test.

The choice matters more for KitCli than for an application, because KitCli
ships as 16 NuGet packages. Whatever it picks becomes a constraint on every
consuming application, not just an internal style preference.

## Decision

KitCli uses non-generic envelopes everywhere and does not use `dynamic` at
all. There are currently zero occurrences of the `dynamic` keyword in the
repository, and zero C# anonymous types.

Eight envelopes carry the pattern:

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

The naming is deliberate: `Anonymous` in a type name means *type-erased*.
It does not mean the same thing as `OutcomeKind.Anonymous`, which means an
outcome has no effect on the workflow run. The two are unrelated.

## Alternatives considered

**`dynamic` at the dispatch points.** The eight `IOutcomeIoWriter`
implementations each pair a `CanWriteFor(Outcome)` type test with an
unchecked cast, which is hand-rolled double dispatch. A single `dynamic`
call site would collapse all of it, and benchmarking of multiple dispatch
in C# finds casting to `dynamic` the most elegant of the available
mechanisms. It lost on three grounds.

- **It removes the compiler as the contract.** A consumer subclassing
  `ArtefactFactory<TOutcome>` is told by the compiler exactly what to
  implement. With `dynamic` there is no interface describing what will be
  called, so a consumer has only the documentation — and if that is
  incomplete, characterisation tests. For a conventions framework, the
  compile-time contract *is* the product.
- **It is not trim- or AOT-compatible.** `dynamic` compiles to late-bound
  `CallSite` invocations with no static references, so the linker cannot
  tell what to preserve. The runtime binder is annotated
  `[RequiresUnreferencedCode]`, so a single use would emit IL2026 into
  every consumer publishing trimmed or Native AOT.
- **It costs more at runtime** than a type test, on every dispatch.

**The visitor pattern.** The classic answer to dispatching on a
heterogeneous collection. Rejected because it forces a cyclic dependency
between the two hierarchies: every new outcome type would require touching
a visitor interface that every existing writer then has to implement.
Envelopes plus a type test leave the two sides independent.

## Consequences

Every public extension point in KitCli is a compile-time contract. A
consumer writing a command, factory, or handler gets a compiler error for
getting it wrong, not a `RuntimeBinderException` in front of a user.

Recovering the type is now the framework's own work, spread across roughly
31 sites: 8 `OfType<>` filters, 7 unchecked casts, 11 `is` tests, and 5
reflection sites rebuilding closed generics. Each is a place a new subtype
can be silently missed rather than failing to compile.

The eight `IOutcomeIoWriter` implementations are the weakest of those.
`ArtefactFactory.Create` recovers its type with `is TOutcome typedOutcome`
and throws a named exception on a mismatch; the writers cast unchecked
after a separate `CanWriteFor` call, so a writer registered against the
wrong outcome throws `InvalidCastException` instead. This is known and
already flagged in source, at
`KitCli.Commands.Abstractions/Io/IOutcomeIoWriter.cs`,
and tracked as [#117](https://github.com/KitCli/KitCli/issues/117).

Avoiding `dynamic` does not by itself make KitCli AOT-safe, and this ADR
should not be read as claiming it does. The five `MakeGenericType` and
`Activator.CreateInstance` sites that [ADR 0003](0003-reflection-based-automatic-registration.md)
introduced are trim-hostile for the same underlying reason `dynamic` is.
No project currently sets `IsTrimmable` or `IsAotCompatible`, so the
warnings have never been measured. Reversing that reflection is a separate
decision, and a much larger one than this.

## References

- [dotnet/linker#1156](https://github.com/dotnet/linker/issues/1156) —
  "The `dynamic` contextual keyword in C# is not friendly for trimming."
- [Prepare .NET libraries for trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)
- [Exploring Multiple Dispatch in C# with BenchmarkDotNet](http://techblog.jetabroad.com/2017/05/exploring-multiple-dispatch-in-c-with.html)
