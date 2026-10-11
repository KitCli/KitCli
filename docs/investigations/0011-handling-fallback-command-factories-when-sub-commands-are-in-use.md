# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity.**

A KitCli app responds to commands someone types, like `/hello`. Some take
it further with a sub-command — an extra word right after, like `/hello
there` — picking one specific action.

Building the object behind one of these asks needs a small piece of setup
code called a factory. Most commands need nothing beyond "create it," so
KitCli writes that trivial one for you automatically — the automatic
fallback.

Writing your own factory, by hand, for just one sub-command currently
switches that automatic fallback off for everything else sharing the same
command word, plain command included.

The obvious-looking fix — leave the automatic fallback switched on too —
was written and tried. It stops the crash. But it also makes a different
sub-command silently build the wrong thing instead, which is worse than
the crash it replaces.

## Recommendation

Three candidate fixes, each a real option with a real cost. Pick one
before turning [#289](https://github.com/KitCli/KitCli/issues/289) into
work to build — the third needs its own estimate, not #289's current
"small fix" label.

1. **Guess from the factory's name.** Every hand-written factory in this
   codebase is already named after the exact thing it builds. Treat that
   as proof of coverage, and fall back automatically for everything else.
   Cheap, but fragile — a rename would silently switch coverage off.
2. **Wait for commands to declare what they need.** Separate, larger work
   is already planned ([#188](https://github.com/KitCli/KitCli/issues/188)).
   It would let a factory state up front, as plain data, which sub-command
   it's for — no guessing. The right long-term answer, but it depends on
   unfinished work not planned with this problem in mind.
3. **Have a factory declare what it builds, not just what it's for** —
   worked through below.

### Option 3, worked through

Today a sub-command's factory only names its command's *word group* —
"hello". It never says which specific thing it builds. So KitCli has no
way to check whether `HelloThereCliCommand` already has a hand-written
factory ("covered") while `HelloMateCliCommand` doesn't ("uncovered") —
both factories, if they existed, would look identical from the outside.

The fix: let a factory declare both things, not just one:

```csharp
public class HelloThereCliCommandFactory
    : CliCommandFactory<HelloCliCommand, HelloThereCliCommand>
```

`HelloCliCommand` is still the word group. `HelloThereCliCommand` is new —
the exact thing this factory builds. Now KitCli can check, for any sibling
command, whether some factory's second type matches it. A match means
covered. No match means uncovered — a real check, not a guess.

That only answers "is it covered?" Say KitCli has worked out that
`HelloMateCliCommand` is uncovered, and needs to build it an automatic
factory. Doing that correctly still needs two answers this check doesn't
give:

- Which word group to register it under — "hello", the one
  `HelloCliCommand` and `HelloThereCliCommand` already use, not something
  built from `HelloMateCliCommand`'s own name, which nobody ever types.
- Which word, typed after "hello", should trigger it — "mate".

Neither comes from the type check above. Both still have to be guessed
from `HelloMateCliCommand`'s name, the same guess option 1 makes.

It's also breaking: every factory written today takes one type, not two.

## What was established

1. **One word group covers a command and all its sub-commands.** `/hello`,
   `/hello there`, and `/hello mate` are all looked up by "hello"; the
   extra word is checked afterwards, by whichever factory is in charge.
2. **That's why nothing today tells a covered sub-command apart from an
   uncovered one.** Every factory in a group shares the same declared
   type. The difference between them lives only inside each factory's own
   logic, invisible until it runs.
3. **Confirmed the reported failure.** One hand-written factory covering
   one sub-command leaves every other ask in its group — plain command
   included — crashing outright, matching the Verdict's claim exactly.

## Evidence

Set up one small family by hand: a plain `HelloCliCommand`, and two
sub-commands, `HelloThereCliCommand` and `HelloMateCliCommand`. Wrote a
factory for `there` only:

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Neither other command needs more than "create it," so neither got a
factory. Typed `/hello`, `/hello there`, and `/hello mate` through
KitCli's actual command-resolution code on `main` (`bbea37e`), then again
after patching it so the automatic fallback is always switched on too:

| Typed | Today | With the fallback always on too |
|---|---|---|
| `/hello` | crashes: "did not find a command factory for hello" | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | crashes, same message as `/hello` | builds `HelloCliCommand` — **wrong command, no crash** |

Option 3 is a design sketch, not yet written or run — nothing has
confirmed it compiles or behaves as described.

## Open questions

- Does this get its own fix, fold into [#188](https://github.com/KitCli/KitCli/issues/188)'s
  larger work, or does option 1 ship first and retire once that work lands?

## Out of scope

- Sub-commands more than one word long — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
