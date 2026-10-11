# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Background, for a reader new to this codebase

A KitCli app responds to commands a person types, like `/hello`. Some
commands take it further with a sub-command — an extra word straight after,
like `/hello there` — that picks one specific action out of several a
command can do.

Building the object that represents one of these asks needs a small piece
of setup code, called a factory. Most of the time nobody writes one: if a
command needs nothing beyond "create it," KitCli writes a trivial one for
you automatically. Call this the **automatic fallback**. You only write
your own factory, by hand, when a command needs something the automatic
one can't give it — reading an argument, say, or deciding between two
different objects to build depending on the sub-command word.

## Verdict

**New complexity.** Writing one hand-written factory for a single
sub-command currently switches the automatic fallback off for every other
ask that shares the same command word — including the plain command with
no sub-command at all. The fix that looks obvious — just leave the
automatic fallback switched on too — was written and tried. It stops the
crash, but it also makes a different sub-command silently build the wrong
thing instead, which is worse than the crash it replaces.

## Recommendation

Three candidate fixes, each a real option with a real cost. Pick one before
turning [#289](https://github.com/KitCli/KitCli/issues/289) into work to
build — the third is big enough a change that it needs its own estimate,
not #289's current "small fix" label.

1. **Guess from the factory's name.** If a hand-written factory's class is
   named after the exact thing it builds — which is already true of every
   factory in this codebase — treat that as proof the thing it builds is
   covered, and give the automatic fallback to everything else in the
   family. Cheap: one small change. Fragile: nothing stops a factory being
   renamed, and a rename would silently switch coverage off again.
2. **Wait for commands to declare what they need.** A separate, larger
   piece of work already planned ([#188](https://github.com/KitCli/KitCli/issues/188),
   via [#190](https://github.com/KitCli/KitCli/issues/190) and
   [#192](https://github.com/KitCli/KitCli/issues/192)) would let a factory
   state up front, as plain data, which sub-command it's for — checkable
   before any command ever runs, with no guessing involved. The right
   long-term answer, but it depends on three pieces of unfinished work that
   weren't planned with this problem in mind.
3. **Have a factory declare what it builds, not just what it's for.**
   Explained in full below — it answers the one question guessing can't,
   but it's a breaking change, and it still needs a guess for part of the
   problem.

### Option 3, in full

Right now, a hand-written factory for a sub-command has to say which
command *word group* it belongs to — "hello" — because that's the only way
KitCli knows to look for it when someone types `/hello` *or* `/hello
there` *or* `/hello mate`. But saying "I'm for the hello word group" is all
it says. It doesn't say which of those three asks it actually builds, and
right now nothing else does either — so KitCli can't tell a covered
sub-command from an uncovered one.

The fix: let a factory declare both things separately — which word group
it's for, and which specific thing it builds:

```csharp
public class HelloThereCliCommandFactory
    : CliCommandFactory<HelloCliCommand, HelloThereCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override HelloThereCliCommand Create() => new HelloThereCliCommand();
}
```

`HelloCliCommand` is still the word group. `HelloThereCliCommand` is new —
the exact thing this factory builds, stated on the class itself. KitCli can
read that straight off the type, with no guessing and without running any
code, so it can finally answer "is `HelloMateCliCommand` already covered?"
with a real check instead of a guess at a factory's name.

That only answers half the problem, though. Once KitCli knows
`HelloMateCliCommand` is *not* covered, it still has to work out two things
neither type parameter tells it: which word group to list its automatic
fallback under (the live one, "hello" — not a dead one nobody ever looks
up), and which sub-command word should trigger it ("mate"). Both still come
from the same kind of guess as option 1 — matching the start of one
command's name against another's. The only way to avoid guessing entirely
is to stop building fallbacks automatically and require a hand-written
factory for every sub-command once any one of them needs one — which
brings back exactly the busywork [#289](https://github.com/KitCli/KitCli/issues/289)
is asking to remove.

It's also a breaking change either way: every factory written today takes
one type, not two. Keeping the single-type form as a shorthand for "builds
exactly what it's for" would spare most of them, but every factory
written for a sub-command — the ones this problem is actually about — would
still need rewriting.

## What was established

1. **A plain command and its sub-commands are looked up the same way.**
   `/hello`, `/hello there`, and `/hello mate` are all looked up by the same
   word, "hello" — the extra word only gets checked afterwards, by whichever
   factory is in charge. (A person typing `/hello there` isn't thought of
   as two words glued together — "hello" is the lookup, "there" is a detail
   checked once something already matched on "hello".)
2. **That's why a sub-command's factory has to share its word group's name.**
   A factory for `/hello there` has to say `HelloCliCommand` — the plain
   command's own type — not `HelloThereCliCommand`, purely so KitCli looks
   for it in the same place it looks for the plain command. Whichever
   factory actually matches is decided afterwards, by asking each
   candidate in turn whether it recognises this specific ask.
3. **So nothing distinguishes "covered" from "not covered."** Every factory
   sharing a word group declares the same thing (the group's own type), so
   there's no reflectable difference between a factory that covers `there`
   and one that would cover `mate` — the only place that difference lives
   today is inside each factory's own decision logic, which can't be
   inspected without running it.
4. **Confirmed the reported failure.** One hand-written factory covering
   one sub-command leaves every other ask sharing its word group with
   nothing to build it — including the plain command with no sub-command —
   and each one crashes outright rather than building anything wrong.
5. **Confirmed the obvious fix trades that crash for something worse.**
   Leaving the automatic fallback switched on everywhere fixes the plain
   command, but an uncovered sub-command stops crashing too — it silently
   builds the plain command instead of the one it should have.

## Evidence

Set up one small family of commands by hand: a plain `HelloCliCommand`, and
two sub-commands, `HelloThereCliCommand` and `HelloMateCliCommand`. Wrote a
factory for `there` only — nothing for the plain command, nothing for
`mate`, on purpose, since neither needs more than "create it":

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Typed `/hello`, `/hello there`, and `/hello mate` against this setup, run
through KitCli's actual command-resolution code on `main` (`bbea37e`), then
again after patching that code so the automatic fallback is always switched
on alongside a hand-written factory:

| Typed | Today | With the fallback always on too |
|---|---|---|
| `/hello` | crashes: "did not find a command factory for hello" | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | crashes, same message as `/hello` | builds `HelloCliCommand` — **wrong command, no crash** |

Option 3 is a design sketch, not yet written or run — unlike the two
columns above, nothing has confirmed it compiles or behaves as described.

## Open questions

- Does this get its own fix, or fold entirely into [#188](https://github.com/KitCli/KitCli/issues/188)'s
  larger piece of work?
- If the name-guessing stopgap (option 1) ships first, does it get retired
  once the larger work lands, or does it stay on as a cheaper permanent
  option?
- If option 3 ships, do the ready-made factories KitCli already provides
  (`BasicCliCommandFactory<T>` and its relatives) stay as they are forever,
  or do they eventually need a two-type form too?

## Out of scope

- Redesigning how a sub-command's factory shares its word group's name —
  settled, already relied on elsewhere, and not what's in question here.
- Sub-commands more than one word long — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
