# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity, and currently blocked.**

A KitCli command is a word someone types after a slash, like `/hello`. A
sub-command is an extra word right after it, like `/hello there`, asking
for one specific action instead of the plain one.

Each command needs a factory: the code that builds the object
representing that exact ask, so KitCli knows which handler runs next.
Most commands don't need a hand-written factory — if building the object
takes nothing beyond `new()`, KitCli writes that factory for you. Call
that the **automatic fallback**.

Writing one hand-written factory, for one sub-command, currently turns
the automatic fallback off for every other command sharing the same
name — the plain command included, and any other sub-command that still
needed nothing but `new()`. Leaving the automatic fallback switched on
regardless was tried as the fix. It stops the plain command from
crashing, but it also makes KitCli silently build the wrong command for
a sub-command nobody wrote a factory for, instead of throwing the
exception it throws today — a wrong answer with no error, worse than
the crash it replaces.

The part of this that's genuinely solvable now — telling a covered
sub-command apart from an uncovered one — is covered below. The part
that isn't: automatically resolving an uncovered sibling with *zero*
hand-written code, which has been the actual requirement since this was
first raised, needs a command to declare what it needs as data. Nothing
in KitCli lets it do that yet. That's milestone
[20, "Describing a Command"](https://github.com/KitCli/KitCli/milestone/20)
— nine open issues, none shipped. Until it lands, every fix available
either guesses from a name (wrong in cases demonstrated below) or asks
for hand-written code the requirement rules out.

## Recommendation

1. **Ship now: tell covered from uncovered by reading a factory's class
   name.** Every hand-written factory in this codebase is already named
   after the exact command it builds — `HelloThereCliCommandFactory`
   builds `HelloThereCliCommand`. Treating a name match as proof of
   coverage needs no new type and breaks nothing. On its own, this fixes
   the reported crash: `/hello` stops crashing once it knows
   `HelloThereCliCommandFactory` covers `there`, not the whole `hello`
   key.
2. **Blocked: resolving an uncovered sibling automatically.** Making
   `/hello mate` work with no factory written for it needs KitCli to
   find `HelloMateCliCommand` on its own — and every way found to do
   that either guesses from names (unreliable in general, not just
   fragile — see Evidence) or needs a developer to hand-write something
   new (ruled out explicitly: see Evidence). Revisit once milestone 20
   ships; that's the one mechanism that's both reliable and free.

Recommend splitting [#289](https://github.com/KitCli/KitCli/issues/289):
ship item 1 as its own small fix now, and either fold the rest into
milestone 20 directly or leave it blocked on it — a call for whoever
reviews this, not decided here.

## What was established

1. **One word covers a command and all its sub-commands.** `/hello`,
   `/hello there`, and `/hello mate` are all looked up by "hello"; the
   extra word is checked afterwards, by whichever factory is in charge.
2. **That's why every factory sharing a command's name has to declare
   that command's own type parameter.** A hand-written factory for
   `/hello there` has to be `CliCommandFactory<HelloCliCommand>`, never
   `CliCommandFactory<HelloThereCliCommand>`, so KitCli looks it up
   under "hello" at all.
3. **So nothing today tells a covered sub-command apart from an
   uncovered one by the type parameter alone** — every factory sharing
   a key declares the identical one. What a factory actually covers
   lives only inside its `CanCreateWhen()` method, unreadable without
   running it.
4. **Confirmed the reported crash and the cost of the naive fix** — see
   Evidence's first table. One dedicated factory covering one
   sub-command left every other ask sharing its key crashing; always
   registering the fallback fixed the crash but made an uncovered
   sibling build silently wrong.
5. **A factory's own class name reliably says what it covers, without
   any new type.** Matching a factory's name (minus a trailing
   "Factory") against real command type names found the right answer
   for every case tried, including the existing codebase's own
   factories — confirmed by re-running the full test suite with
   detection switched from type-matching to name-matching: no
   regressions.
6. **Finding an *uncovered* sibling automatically needs the same kind
   of name match, and that one isn't reliable.** Grouping a command
   with the longest other command name it starts with found a false
   match in this codebase's own test suite — two unrelated private
   fixtures, both named `TestCliCommand` in different files — before a
   guard was added requiring a strict length difference. The guard
   fixes that specific case, not the general one: see fact 7.
7. **The general case can't be fixed by a better guard.** A command's
   own name can be several words before any sub-command word is added
   (e.g. `HelloTherePersonCliCommand` for `/hello-there-person`, no
   sub-command). Given a longer name built on top of that —
   `HelloTherePersonSubCliCommand` — there's no way to tell from the
   string alone whether the split is `Hello` + `TherePersonSub`,
   `HelloThere` + `PersonSub`, or `HelloTherePerson` + `Sub`. Checking
   which candidate exists as a real type only works when the shorter
   command was written as its own type — and that's exactly the
   zero-code case nothing can be checked against.
8. **That ambiguity doesn't reach a sub-command that already has a
   hand-written factory.** A factory using today's shared-key pattern
   has to declare the primary command's real type as its own type
   parameter to compile at all — so for anything with a hand-written
   factory, the primary command's existence is a guaranteed fact, not a
   guess. The ambiguity only bites the fully-automatic, nothing-written
   case.
9. **A factory declaring two things — its shared key and the exact type
   it builds — works, and was rejected anyway.** Giving
   `CliCommandFactory<T>` a second type parameter made "is this
   specific sibling covered" a real, reflectable fact instead of a
   guess, and the full suite passed with it in place (see Evidence).
   Rejected on two separate grounds: it's a breaking change to every
   factory written today, and it's new ceremony a developer has to
   hand-write for any sub-command that wants to be found this way —
   ruled out as a requirement, not a preference.
10. **No mechanism available today is both reliable and free — only
    milestone 20 would be.** [#190](https://github.com/KitCli/KitCli/issues/190)
    would let a factory declare its sub-command requirement as plain
    data, readable before any instruction runs — a real fact, not a
    guess, and nothing new to hand-write beyond stating the requirement
    the factory needs anyway. [#192](https://github.com/KitCli/KitCli/issues/192)
    would make `CanCreateWhen()` read that declaration instead of being
    a black box. Both are still open.

## Evidence

Wrote one small family by hand: a plain `HelloCliCommand`, and two
sub-commands, `HelloThereCliCommand` and `HelloMateCliCommand`. Wrote a
factory for `there` only — the other two need nothing beyond `new()`:

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Typed `/hello`, `/hello there`, and `/hello mate` through KitCli's actual
command-resolution code on `main` (`bbea37e`), then again after removing
the guard that stops the automatic fallback registering whenever a
dedicated factory already exists:

| Typed | Today | With the automatic fallback always registered |
|---|---|---|
| `/hello` | throws `NoCommandGeneratorException: Did not find command factory for hello` | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | throws the same exception as `/hello` | builds `HelloCliCommand` — **wrong command, no exception** |

Rewrote `HelloThereCliCommandFactory` to declare both a shared key and
the exact type it builds — `CliCommandFactory<HelloCliCommand,
HelloThereCliCommand>` instead of `CliCommandFactory<HelloCliCommand>` —
and had registration read the second type by reflection instead of
guessing. This worked: the full solution's test suite passed, including
`/hello`, `/hello there`, and `/hello mate` all resolving correctly with
zero code written for `/hello mate`. Reverted anyway — see established
fact 9.

Reran the whole `KitCli.Commands.Abstractions.Tests` project — 148
tests, unrelated to this investigation — while building the
name-matching grouping described in fact 6. A pre-existing pair of
fixtures, `CliCommandTests.cs`'s private `TestCliCommand` and
`Handlers/NoCliCommandHandlerTests.cs`'s private `TestCliCommand`,
share an identical stripped name ("Test") and were matched against
each other as siblings before a strict-length guard was added. Neither
test calls command registration directly; both were swept in because
registration scans every `CliCommand` type in the whole assembly,
regardless of which test is running.

## Open questions

- Does [#289](https://github.com/KitCli/KitCli/issues/289) split into
  the small fix (recommendation 1) shipping now and the rest folding
  into milestone 20, or does the whole issue just wait?
- Is the name-matching detection in recommendation 1 worth keeping once
  milestone 20 ships, or does it get replaced outright once a factory
  can declare its requirement properly?

## Out of scope

- Sub-commands more than one word long *after* the command name (e.g.
  `/user edit cir`) — a different ambiguity, already covered by
  investigation [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
  This investigation's ambiguity (established fact 7) is about the
  command's own name being multiple words, not what follows it.
- Redesigning the shared-key pattern itself — settled, already relied
  on elsewhere, not in question here.
