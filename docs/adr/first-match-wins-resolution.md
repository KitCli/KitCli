# Resolve ambiguous candidates by first match in registration order

Status: Accepted (retroactive — reconstructed from current source, not
original notes)
Date: 2026-07-26
Release: 1.0.0

## Context

Four independent parts of KitCli face the same question: given a runtime
value and a list of candidates that each declare whether they *can* handle
it, which one does, when more than one says yes?

- `InstructionParser.Parse` picks the first `IInstructionArgumentBuilder`
  whose `For(value)` returns `true`
  ([instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md)).
- `CliWorkflowCommandProvider.GetCommand` picks the first
  `ICliCommandFactory`, among those keyed under the resolved name, whose
  `CanCreateWhen()` returns `true`
  ([command-registration.md](../concepts/command-registration.md)).
- The same provider's `ConvertOutcomesToArtefacts` picks the first
  `IArtefactFactory` whose `For(outcome)` returns `true`
  ([artefacts.md](../concepts/artefacts.md)).
- `CliApp.WriteOutcomes` picks the first `IOutcomeIoWriter` whose
  `CanWriteFor(outcome)` returns `true`
  ([outcome-writing.md](../concepts/outcome-writing.md)).

Each was arrived at separately, and all four behave the same. Naming it as
one pattern means learning it once.

## Decision

Wherever KitCli chooses among candidates that self-report "can I handle
this", take the first that says yes — by DI registration order, or by list
order where the caller controls it. No priority system, no ambiguity error,
no detection of a second candidate that would also have said yes.

## Alternatives considered

- **Throw on ambiguity** — safer, but requires evaluating every candidate
  rather than short-circuiting, and turns an overlapping registration into
  a runtime crash. Command factory registration already throws on
  *type*-level ambiguity at startup; the four points here do not.
- **Explicit priority metadata on each candidate** — removes the
  order dependency, and adds a second thing every implementer must get
  right on top of the `For`/`CanCreateWhen` check.
- **Keyed lookup instead of scan-and-test** — works only where a natural
  string key exists, which is why command resolution narrows by name
  *before* falling back to this pattern. The other three resolve from a
  runtime value, not a string a user typed.

## Consequences

All four points inherit the same fragility: correctness depends on
registration order, and that order lives in whichever `Add...` call wired
things up, not in the candidate types. Moving or inserting a registration
can silently change which candidate wins, with no error anywhere. This is
an accepted cost of keeping registration out of consuming code
(the [reflection-registration ADR](reflection-based-automatic-registration.md)), not an
oversight repeated four times.
