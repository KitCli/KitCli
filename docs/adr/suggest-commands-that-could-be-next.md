# Suggest commands that could be next via a `[CliNextCommandCouldBe]` attribute

Status: Proposed
Date: 2026-10-10
Release: 3.2.0

## Context

The [next-command ADR](suggest-next-commands-attribute.md) added
`[CliNextCommandIs]`: declare a command's likely next step, and the run
suggests it if a later ask doesn't resolve to anything. That only happens
while a run is still open, waiting on one more valid ask.

A command that ends its run outright — a listing command, say — never gets
that second ask, so it can never suggest anything.
[#294](https://github.com/KitCli/KitCli/issues/294) raises this: `/intend`
lists things and finishes; it has no way to hint that
`/intend to --choose` is the obvious next step.

#294 proposes a second attribute rather than widening `CliNextCommandIs`,
since `Is` reads as "the run expects one of these" — a claim a one-shot
command's hint isn't making.

## Decision

Add `CliNextCommandCouldBeAttribute(name, description)`, the same shape as
`CliNextCommandIsAttribute`. Unlike `Is`, it prints right after a command
finishes, whether or not the run stays open. A command may declare both:

```csharp
[CliNextCommandIs("next", "Show the next page.")]
[CliNextCommandCouldBe("export", "Export the current results.")]
public record ShowPageCliCommand : CliCommand;
```

- `Is` — the run is still open and the last thing typed didn't resolve;
  here's what would.
- `CouldBe` — a command just finished; here's what's often asked next.

Both now print under a header line instead of bare, so a reader knows why
they're seeing suggestions at all: `"You might have meant:"` for `Is`,
`"All done. You might want to run:"` for `CouldBe`. That header needs a
blank line in front of it, so a small `PauseOutcome` type exists again —
the [next-command ADR](suggest-next-commands-attribute.md) had rejected this exact
type for having only one use; it now has two.

## Alternatives considered

- **A flag on `CliNextCommandIsAttribute`** marking a suggestion
  non-binding. Rejected: the two cases still need separate handling
  wherever they're read, so the flag buys nothing but an attribute whose
  meaning depends on an argument.
- **Widen `Is` to fire on every finish** — the option #294 itself raises.
  Rejected: a run that's still open would print the same suggestion twice
  in one exchange, once on finishing and once when the next ask fails.
- **One shared entry point for both attributes.** Rejected: `Is` needs to
  look back at what already ran; `CouldBe` fires straight off the command
  that just ran, with nothing to look back at.
- **No header, just the suggestions.** Tried first; read as noise with no
  explanation. One line of header earns its keep.

## Consequences

- Two attributes to know instead of one, each with one fixed meaning.
- A command can declare both: `CouldBe` shows once on finishing, `Is`
  shows again only if the run stays open and the next ask fails — never
  for the same event, so never twice.
- `PauseOutcome` exists again, reversing that part of the next-command ADR. The next-command ADR's
  core decision — the `[CliNextCommandIs]` attribute itself — still
  stands.
