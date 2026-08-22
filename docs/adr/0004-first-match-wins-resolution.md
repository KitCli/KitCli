# 0004. Resolve ambiguous candidates by first match in registration order

Status: Accepted (retroactive — reconstructed from current source, not
original notes)
Date: 2026-07-26

## Context

Several independent parts of KitCli face the same shape of problem: given
a runtime value and a collection of candidate handlers that each declare
whether they *can* handle it, decide which one actually does when more
than one says yes.

- `InstructionParser.Parse` picks the first `IInstructionArgumentBuilder`
  whose `For(argumentValue)` returns `true`, out of a fixed registration
  order ending in `BoolInstructionArgumentBuilder`'s unconditional `true`
  (see [instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md)).
- `CliWorkflowCommandProvider.GetCommand` picks the first
  `ICliCommandFactory` (among those keyed under the resolved instruction
  name) whose `CanCreateWhen()` returns `true` (see
  [command-registration.md](../concepts/command-registration.md)).
- The same provider's `ConvertOutcomesToArtefacts` picks the first
  `IArtefactFactory` whose `For(outcome)` returns `true` (see
  [artefacts.md](../concepts/artefacts.md)).
- `CliApp.WriteOutcomes` picks the first `IOutcomeIoWriter` whose
  `CanWriteFor(outcome)` returns `true` (see
  [cli-app-host.md](../concepts/cli-app-host.md)).

Each of these was arrived at independently as each subsystem was built,
but they all resolve the same way. That consistency is worth naming as a
deliberate pattern rather than four unrelated coincidences, so a
contributor who's learned one of them already knows how the other three
behave.

## Decision

Wherever KitCli must choose among candidates that each self-report
"can I handle this," take the first one (by DI registration order, or by
list order where the caller controls it directly) that says yes. No
priority system, no ambiguity error, no attempt to detect or warn about a
second candidate that would also have said yes.

## Alternatives considered

- **Throw on ambiguity** — safer (a genuine conflict fails loudly instead
  of silently picking one), but would require evaluating *every* candidate
  before deciding instead of short-circuiting on the first match, and
  would turn "I added a builder/factory that overlaps an existing one"
  into a runtime crash rather than a silent behavior change — a tradeoff
  KitCli hasn't made in either direction consistently (command factory
  registration *does* throw on type-level ambiguity at startup — see
  [command-registration.md](../concepts/command-registration.md) — while
  every other resolution point here doesn't).
- **Explicit priority/ordering metadata on each candidate** — removes the
  registration-order dependency, but adds a second thing (the priority
  value) every implementer has to get right, on top of the `For`/`CanCreateWhen`
  check itself.
- **Keyed lookup instead of scan-and-test** — works when there's a natural
  string key (command names), which is why command factory resolution
  narrows by key *before* falling back to this pattern for the remaining
  tie. It doesn't work for the other three cases, which resolve from a
  runtime value/type, not a string a user typed.

## Consequences

Every one of these four resolution points inherits the same fragility:
correctness depends on registration order, and that order lives in
whichever `AddXFromAssembly`/`AddCliXBuilders` call wired things up — not
in the candidate types themselves. Moving or inserting a registration can
silently change which candidate wins for values both would have claimed,
with no error at any point to catch it. This is a known, accepted
tradeoff for keeping each subsystem's registration free of consuming code
(see [0003](0003-reflection-based-automatic-registration.md)), not an
oversight repeated four times independently.
