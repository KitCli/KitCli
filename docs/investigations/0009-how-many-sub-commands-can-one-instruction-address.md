# 0009. How many sub-commands can one instruction address?

- **Status:** In Review
- **Spike:** not yet filed
- **Time-box:** none agreed — ran inline
- **Date:** 2026-09-11

## Verdict

**New complexity.** The depth limit is not real: every word between the
command name and the first `--` is already captured, so
`/user edit cir --value hello` arrives whole. It just arrives glued into
one string, which nothing can match against.

The complexity is knowing which word the list ends on. In `/user edit cir`,
`edit` names a command and `cir` is a value — and reading left to right,
nothing tells them apart. Both are plain words. KitCli has no concept of a
value without a `--name` in front of it, so `cir` has nowhere to go. That
is a second deliverable, not a consequence of the first.

## Recommendation

Three tickets, in this order, under *Describing a Command*.

1. **Carry the words as a list.** `Instruction` holds ordered segments
   instead of one sub-name string, the indexer stops joining them, and
   `SubCommandIs` compares a sequence. Breaking change to a public record,
   so an ADR rides along in the same pull request.
2. **Teach the descriptor a word path** — [#190](https://github.com/KitCli/KitCli/issues/190)'s
   `SubCommandIs(name)` accepts several names.
3. **Positional arguments, on their own ticket.** A command declares how
   many unnamed values follow its path. Decide alongside
   [#244](https://github.com/KitCli/KitCli/issues/244) — both replace
   guessing from text with a declaration.

## What was established

1. **KitCli already defers this decision to the command.** The parser hands
   over an instruction; `CliWorkflowCommandProvider` then finds factories by
   command name and asks each one `CanCreateWhen()`. So the word path can be
   matched at factory selection. No tree is needed while parsing.
2. **That is how the field does it.** [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax)
   splits input into tokens that carry no meaning, and leaves it to the app
   being invoked to say which are commands and which are values. Its
   sub-commands nest without limit.
3. **The matching rule is short and settled.** [Cobra](https://github.com/spf13/cobra/blob/main/command.go)
   drops the flags, and if the first remaining word names a child command it
   recurses into it; the first word that doesn't ends the path, and the rest
   are values. [Clap](https://deepwiki.com/clap-rs/clap/7.3-subcommands)
   breaks ties toward the command.
4. **Guessing without the tree is the known failure.** Python's
   [argparse](https://docs.python.org/3/library/argparse.html) lets an
   optional value swallow the sub-command name, then reports the sub-command
   as missing.

## Evidence

`TestSubCommandPathCliCommand` in the playground says back whatever
sub-command name it was handed. Run it against `3d282e7`:

```
dotnet run --project KitCli.Playground.App.Headless -- "/test-sub-command-path user edit cir"

Sub-command name: "user edit cir"
```

Three words, one string — and afterwards nothing can tell which of them
named a command and which was a value.

## Open questions

- How does a wrong word report? `/user edt cir` and `/user edit cir extra`
  both end the path early and look identical to a missing command.
- Does this enter through the Ideas board for a WAG, or straight onto the
  *Describing a Command* milestone alongside #190?

## Out of scope

- **Quoting.** `"hello!"` keeps its quote marks. Already
  [#39](https://github.com/KitCli/KitCli/issues/39).
- **How commands are registered.** Factories stay keyed on the first word
  only; nothing here changes [ADR 0004](../adr/0004-first-match-wins-resolution.md).
