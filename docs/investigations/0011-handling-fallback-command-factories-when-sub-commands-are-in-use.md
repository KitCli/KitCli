# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity, and currently blocked.**

A KitCli command is a word typed after a slash, like `/hello`. A
sub-command is an extra word right after it, like `/hello there`, asking
for one specific action instead of the plain one. A sub-command needs a
factory — code that builds it — only when it needs more than `new()`;
otherwise KitCli writes that factory automatically. Today, writing one
hand-written factory for one sub-command (`there`) turns the automatic
factory off for every other command sharing that word — including the
plain command and any other sub-command (`mate`) that still needed
nothing but `new()`.

Finding that `HelloMateCliCommand` has no factory isn't the problem —
that's a one-line check: scan every command type, subtract the ones a
factory already covers, whatever's left is uncovered. The actual
unknown is narrower: nothing records that `HelloMateCliCommand` belongs
to `HelloCliCommand`, so there's no fact to read for what the automatic
fallback would need — `CliCommandFactory<HelloCliCommand>` as its type,
`SubCommandIs("mate")` as its check. That relationship can only be
guessed from the two names today, and the guess is wrong in general,
not just fragile (established facts 4–5).

Fixing that needs a command to declare its relationship as data instead
of KitCli guessing it. Nothing in KitCli does that yet — it's milestone
[20, "Describing a Command"](https://github.com/KitCli/KitCli/milestone/20),
nine open issues, none shipped.

## Recommendation

1. **Ship now: tell a covered command from an uncovered one by reading
   a factory's class name.** Every hand-written factory here is already
   named after what it builds — `HelloThereCliCommandFactory` builds
   `HelloThereCliCommand`. No new type, nothing breaks, and on its own
   this fixes the reported crash: `/hello` stops failing once KitCli
   knows `HelloThereCliCommandFactory` only covers `there`.
2. **Blocked: declaring an uncovered sibling's relationship.** Nothing
   available today can produce `HelloCliCommand` + `"mate"` for
   `HelloMateCliCommand` both reliably and for free — only guess it
   (wrong in general) or hand-write it (ruled out as a requirement, not
   a preference). Revisit once milestone 20 ships.

Recommend splitting [#289](https://github.com/KitCli/KitCli/issues/289):
ship item 1 now, leave the rest blocked on milestone 20 — a call for
whoever reviews this.

## What was established

1. **One word looks up a command and all its sub-commands together,
   which is why every sub-command's factory has to declare the plain
   command's own type.** A hand-written factory for `/hello there` has
   to be `CliCommandFactory<HelloCliCommand>`, never
   `CliCommandFactory<HelloThereCliCommand>` — otherwise KitCli
   wouldn't look it up under "hello" at all. So nothing distinguishes a
   covered sub-command from an uncovered one by that type alone; what a
   factory actually covers lives only inside its `CanCreateWhen()`
   method, unreadable without running it.
2. **Confirmed the reported crash, and that the obvious fix is worse.**
   One factory covering one sub-command left every other ask sharing
   its key crashing (Evidence). Always registering the automatic
   fallback regardless stopped the crash, but made the uncovered
   sibling build silently *wrong* instead — a worse failure than the
   crash it replaced.
3. **A factory's own class name safely reveals what it covers — no new
   type needed.** Matching a factory's name, minus "Factory", against
   real command type names got the right answer everywhere tried,
   including this codebase's own factories, with no regressions across
   the full test suite.
4. **Guessing an uncovered sibling's relationship from names finds
   false matches even in this codebase.** Grouping a command with the
   longest other command name it starts with matched two unrelated
   private `TestCliCommand` fixtures, in different test files, against
   each other — their names happened to be identical. A guard (require
   a strictly longer name) fixed that one case, not the general
   problem (fact 5).
5. **The general problem: a command's own name can be several words
   before any sub-command word is added, so the split is genuinely
   ambiguous.** Given `HelloTherePersonSubCliCommand`, nothing says
   whether it's `Hello` + `TherePersonSub`, `HelloThere` + `PersonSub`,
   or `HelloTherePerson` + `Sub`. Checking which candidate exists as a
   real type only resolves it when the shorter command was written as
   its own type — which fails exactly when there's no factory to check
   against, the case this is meant to solve. (This doesn't reach a
   sub-command that already has a hand-written factory — that factory
   has to declare the real primary type to compile, so its existence
   is a guaranteed fact there, not a guess.)
6. **A factory declaring both its key and what it builds works, and
   was rejected anyway.** Giving `CliCommandFactory<T>` a second type
   parameter made "is this sibling covered" a real, reflectable fact
   instead of a guess — the full suite passed with it in place
   (Evidence). Rejected on two grounds: breaking change to every
   existing factory, and new ceremony a developer has to hand-write —
   ruled out as a requirement.
7. **The one fix that's both reliable and free isn't built yet.**
   [#190](https://github.com/KitCli/KitCli/issues/190) would let a
   factory declare its sub-command requirement as data, read before
   any instruction runs. [#192](https://github.com/KitCli/KitCli/issues/192)
   would make `CanCreateWhen()` read that declaration instead of being
   a black box. Both open.

## Evidence

One family by hand: `HelloCliCommand`, `HelloThereCliCommand`,
`HelloMateCliCommand`, with a factory for `there` only:

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Typed `/hello`, `/hello there`, `/hello mate` through KitCli's actual
command-resolution code on `main` (`bbea37e`), then again with the
automatic fallback always registered regardless of other factories:

| Typed | Today | Fallback always registered |
|---|---|---|
| `/hello` | throws `NoCommandGeneratorException` | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | throws the same exception | builds `HelloCliCommand` — **wrong, no exception** |

Rewrote the factory as `CliCommandFactory<HelloCliCommand,
HelloThereCliCommand>` and had registration read the second type by
reflection. Worked — full suite passed, `/hello mate` resolved with
zero code written for it. Reverted anyway (established fact 6).

While building the name-matching grouping (fact 4), reran
`KitCli.Commands.Abstractions.Tests` (148 tests, unrelated to this
investigation) and found `CliCommandTests.cs`'s private `TestCliCommand`
matched against `Handlers/NoCliCommandHandlerTests.cs`'s private
`TestCliCommand` — different files, identical stripped name. Neither
test calls registration directly; both were swept in because
registration scans every command type in the assembly regardless of
which test is running.

## Open questions

- Does [#289](https://github.com/KitCli/KitCli/issues/289) split —
  recommendation 1 now, the rest blocked on milestone 20 — or does the
  whole issue wait?
- Does the name-matching detection in recommendation 1 survive once
  milestone 20 ships, or get replaced outright?

## Out of scope

- Sub-commands more than one word long *after* the command name (e.g.
  `/user edit cir`) — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
  This investigation's ambiguity (fact 5) is about the command's own
  name being multiple words, a different axis.
- Redesigning the shared-key pattern itself — settled, relied on
  elsewhere.
