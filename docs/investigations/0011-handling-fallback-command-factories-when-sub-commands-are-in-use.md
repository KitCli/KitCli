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
unknown is narrower, and more fundamental than "hard to find": nothing
ever asserts that `HelloMateCliCommand` *is* a sub-command of
`HelloCliCommand` in the first place. That fact only exists as the
behaviour of a `CanCreateWhen()` method someone wrote — specifically, a
call to `SubCommandIs("mate")`. No factory means no such call was ever
written, so there's nothing to read, not even something hidden. The
only way to produce `CliCommandFactory<HelloCliCommand>` as the type
and `SubCommandIs("mate")` as the check is to guess them from the two
names, and the guess is wrong in general, not just fragile (established
facts 4–5).

Fixing that needs a command to declare its relationship as data instead
of KitCli guessing it. Nothing in KitCli does that yet — not even
milestone [20, "Describing a Command"](https://github.com/KitCli/KitCli/milestone/20)
as currently scoped (established fact 8).

## Recommendation

1. **Ship now: tell a covered command from an uncovered one by reading
   a factory's class name.** Every hand-written factory here is already
   named after what it builds — `HelloThereCliCommandFactory` builds
   `HelloThereCliCommand`. No new type, nothing breaks, and on its own
   this fixes the reported crash: `/hello` stops failing once KitCli
   knows `HelloThereCliCommandFactory` only covers `there`.
2. **Decided: ship the guess now too, despite being unreliable in
   general.** Nothing available today can produce `HelloCliCommand` +
   `"mate"` for `HelloMateCliCommand` both reliably and for free — only
   guess it (wrong in general, established facts 4–5) or hand-write it
   (ruled out as a requirement). Shipping the guess anyway trades the
   bug reported in #289 — near-certain, fires on every hand-written
   sub-command factory anyone writes — for a narrower one that needs a
   specific naming coincidence between two unrelated commands, and
   fails the same loud way (established fact 9). Net improvement.
3. **Still blocked: removing the guess entirely.** That needs milestone
   20 to gain the addition in established fact 8. Revisit once it does.

Recommend [#289](https://github.com/KitCli/KitCli/issues/289) ships as
recommendation 2 describes, with recommendation 3 tracked separately
against milestone 20 — a call for whoever reviews this.

## What was established

1. **One word looks up a command and all its sub-commands together,
   which is why every sub-command's factory has to declare the plain
   command's own type.** A hand-written factory for `/hello there` has
   to be `CliCommandFactory<HelloCliCommand>`, never
   `CliCommandFactory<HelloThereCliCommand>` — otherwise KitCli
   wouldn't look it up under "hello" at all. So nothing distinguishes a
   covered sub-command from an uncovered one by that type alone; what a
   factory actually covers lives only inside its `CanCreateWhen()`
   method. For an uncovered command, that method was never written at
   all — there's nothing to read, not even something unreadable.
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
   or `HelloTherePerson` + `Sub`. The deeper reason it's unresolvable:
   nothing marks `HelloTherePersonSubCliCommand` as a sub-command of
   anything in the first place, so there's no authority to check a
   candidate split against beyond hoping a shorter type happens to
   exist. Checking which candidate exists as a real type only resolves
   it when the shorter command was written as its own type — which
   fails exactly when there's no factory to check against, the case
   this is meant to solve. (This doesn't reach a
   sub-command that already has a hand-written factory — that factory
   has to declare the real primary type to compile, so its existence
   is a guaranteed fact there, not a guess.)
6. **A factory declaring both its key and what it builds works for
   anything a developer wrote, and was rejected anyway.** Giving
   `CliCommandFactory<T>` a second type parameter made "is this
   sibling covered" a real, reflectable fact instead of a guess — but
   only for a factory someone hand-wrote. It did nothing for
   `HelloMateCliCommand`, which still needed the same guess as fact 4
   (Evidence). Rejected on two further grounds: breaking change to
   every existing factory, and new ceremony a developer has to
   hand-write — ruled out as a requirement.
7. **The one fix that's both reliable and free isn't built yet.**
   [#190](https://github.com/KitCli/KitCli/issues/190) would let a
   factory declare its sub-command requirement as data, read before
   any instruction runs. [#192](https://github.com/KitCli/KitCli/issues/192)
   would make `CanCreateWhen()` read that declaration instead of being
   a black box. Both open.
8. **Checked every issue in milestone 20, and none of them cover a
   command declaring a relationship to *another* command.** #190's
   `SubCommandIs(name)` is declared on a factory, which
   `HelloMateCliCommand` doesn't have. #191 gives a factory-less
   command a descriptor too, but only from attributes already
   planned — aliases, next-command hints — not a new one for "which
   command is this a sub-command of." #264 (a human-readable
   description) and #265 (a listing of registered commands) are
   unrelated. The natural shape for the missing piece follows #191's
   own precedent for the factory-less case: an attribute on the
   primary command, since there's no factory to call a builder method
   on — `[HasSubCommand(typeof(HelloMateCliCommand), "mate")]` on
   `HelloCliCommand`, not a method milestone 20's planned
   `OnDescribing` hook would have nowhere to live for a factory-less
   command.
9. **The guess trades a common, certain bug for a rarer, narrower one —
   confirmed lower severity, not just assumed.** If a command's own
   name happens to be an exact prefix of an unrelated, multi-word
   command's name elsewhere in the same app, that command stops being
   reachable under its own name — a real regression this fix
   introduces. But #289 fires on *every* hand-written sub-command
   factory anyone writes, a routine thing to do; this needs a specific
   naming coincidence between two otherwise-unrelated commands, which
   most command sets will never have. Both fail the same way too — a
   loud crash on a name that used to resolve, not a silent wrong
   answer — so this isn't a new *kind* of failure, just a narrower one.

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
reflection, instead of guessing `HelloThereCliCommand`'s coverage from
the factory's class name. That detection worked reliably. `/hello
mate` still needed the same name-matching guess as fact 4 to be found
and given a relationship — the two-parameter factory only makes
something a developer *wrote* checkable; it does nothing for a sibling
nobody wrote anything for. Full suite passed regardless. Reverted
anyway (established fact 6).

While building the name-matching grouping (fact 4), reran
`KitCli.Commands.Abstractions.Tests` (148 tests, unrelated to this
investigation) and found `CliCommandTests.cs`'s private `TestCliCommand`
matched against `Handlers/NoCliCommandHandlerTests.cs`'s private
`TestCliCommand` — different files, identical stripped name. Neither
test calls registration directly; both were swept in because
registration scans every command type in the assembly regardless of
which test is running. This specific pair was renamed apart
(`NoHandlerCliCommand`) to remove the one instance found — the general
risk this demonstrates (fact 4) remains for any two types that happen
to share a name elsewhere.

## Open questions

- Does the name-matching detection in recommendation 1 survive once
  milestone 20 ships, or get replaced outright?
- Does fact 8's addition get proposed as a new issue under milestone
  20, or folded into #191 directly, since it extends the exact
  precedent #191 already sets?

## Out of scope

- Sub-commands more than one word long *after* the command name (e.g.
  `/user edit cir`) — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
  This investigation's ambiguity (fact 5) is about the command's own
  name being multiple words, a different axis.
- Redesigning the shared-key pattern itself — settled, relied on
  elsewhere.
