# 0018. Suggest commands that could be next via a `[CliNextCommandCouldBe]` attribute

Status: Proposed
Date: 2026-10-10

## Context

[ADR 0008](0008-suggest-next-commands-attribute.md) added
`[CliNextCommandIs]`, read by `CliWorkflowRun.RejectAsk`, which only runs
when the run has already parked at `ReachedReusableOutcome` and the next
typed input resolves to nothing. A command that finishes on a non-reusable
outcome never reaches that path — raised as
[#294](https://github.com/KitCli/KitCli/issues/294): a listing command
that ends the run, `/intend` say, cannot hint at `/intend to --choose` the
way a paging command hints at `/next`.

#294 proposes a second attribute rather than widening `CliNextCommandIs`,
because `Is` reads as "the run expects one of these", a sequencing claim a
one-shot command's hint is not making.

## Decision

Add `CliNextCommandCouldBeAttribute(name, description)` — same shape as
`CliNextCommandIsAttribute`, same `AllowMultiple`. It is read from a new
call site, not the one `Is` uses: right after `ExecuteCommand` resolves a
command, whatever outcome the run reached, print its declared `CouldBe`
suggestions once. That call site already holds the just-run `CliCommand`,
so `SuggestNextCommands` gains an overload taking it directly instead of
re-deriving it from `AllPriorOutcomes().OfType<RanCliCommandOutcome>()`.

The two attributes split by when they fire, not by what they look like:

- `[CliNextCommandIs]` — you're parked and typed something that didn't
  resolve; here's what moves the run forward.
- `[CliNextCommandCouldBe]` — you've just run a command; here's what's
  often asked next.

A command may declare both. `Is` keeps its existing call site and
`List<Outcome>` overload, but both overloads of `SuggestNextCommands` now
lead with a one-line header, each worded for when it fires:

- `Is`/`RejectAsk`: `PauseOutcome`, then `SayOutcome("You might have
  meant:")` — the run is still open, the ask just didn't resolve.
- `CouldBe`: `PauseOutcome`, then `SayOutcome("All done. You might want
  to run:")` — the command just finished.

`PauseOutcome` is a new outcome: a blank line, with its own
`PauseOutcomeIoWriter`. [ADR 0008](0008-suggest-next-commands-attribute.md)
considered and rejected exactly this type, folding the blank line into
`SuggestionOutcomeIoWriter` instead because it had no other consumer. It
now has one — the blank line before the header — so it is reintroduced as
its own type rather than duplicating `cliIo.Pause()` at a second call
site. `SuggestionOutcomeIoWriter` keeps calling `cliIo.Pause()` directly
for the blank line between suggestions; it is not changed to emit
`PauseOutcome` itself, since nothing asked for that and it still works.

## Alternatives considered

- **A bool flag on `CliNextCommandIsAttribute`** marking a candidate
  non-binding. Rejected as a boolean-flag smell: the two modes still need
  separate handling at the call site, so the flag buys no simplification,
  only an attribute whose meaning depends on an argument.
- **Widen `[CliNextCommandIs]`'s scope to fire on every finish**, the
  option #294 itself raises and sets aside. Rejected here for a concrete
  failure mode: a parked run would print the same suggestion twice in one
  exchange — once when the command completes, again when the next ask
  fails to resolve.
- **One call site for both attributes.** Rejected: `Is` depends on history
  already recorded by the time `RejectAsk` runs; `CouldBe` fires once,
  straight off the command just run, with no history lookup needed.
  Merging them reintroduces the bool-flag problem one level up.
- **No header line, just the suggestions.** The first cut did this; the
  suggestion block read as unstructured noise with nothing to say what it
  was. A header line split by context (`Is` vs `CouldBe`) is one line of
  cost for a reader to know why they're seeing it.

## Consequences

- Two attributes to know instead of one, each with a single fixed
  meaning instead of a mode flag.
- `SuggestNextCommands` gains a `CliCommand` overload, used only by
  `CouldBe`'s call site.
- A command declaring both attributes shows `CouldBe` once on finishing,
  and `Is` again only if the run parks and the next ask fails — no
  duplication, because the two never print for the same event.
- `PauseOutcome` exists again, reversing that part of ADR 0008. ADR 0008's
  core decision — the `[CliNextCommandIs]` attribute itself — is
  unaffected and stays Accepted.
