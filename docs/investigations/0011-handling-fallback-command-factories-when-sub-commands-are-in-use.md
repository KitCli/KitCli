# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity.** A KitCli app responds to commands someone types, like
`/hello`. Some take it further with a sub-command — an extra word right
after, like `/hello there` — picking one specific action. Building the
object behind one of these asks needs a small piece of setup code called a
factory; most commands need nothing beyond "create it," so KitCli writes
that trivial one for you automatically. Writing your own factory, by hand,
for just one sub-command currently switches that automatic fallback off for
everything else sharing the same command word — plain command included.
The obvious-looking fix — leave the automatic fallback switched on too —
was written and tried. It stops the crash, but silently makes a different
sub-command build the wrong thing instead, worse than the crash it replaces.

## Recommendation

Three candidate fixes, each a real option with a real cost. Pick one before
turning [#289](https://github.com/KitCli/KitCli/issues/289) into work to
build — the third needs its own estimate, not #289's current "small fix"
label.

1. **Guess from the factory's name.** Every hand-written factory in this
   codebase is already named after the exact thing it builds. Treat that as
   proof of coverage, and fall back automatically for everything else.
   Cheap, but fragile — a rename would silently switch coverage off.
2. **Wait for commands to declare what they need.** Separate, larger work
   already planned ([#188](https://github.com/KitCli/KitCli/issues/188),
   via [#190](https://github.com/KitCli/KitCli/issues/190) and
   [#192](https://github.com/KitCli/KitCli/issues/192)) would let a factory
   state up front, as plain data, which sub-command it's for — no guessing.
   The right long-term answer, but it depends on three unfinished pieces of
   work that weren't planned with this problem in mind.
3. **Have a factory declare what it builds, not just what it's for.** Today
   a sub-command's factory only names its command's *word group*
   ("hello"), never the specific thing it builds — so nothing can tell a
   covered sub-command from an uncovered one. Fix: let it declare both, e.g.
   `CliCommandFactory<HelloCliCommand, HelloThereCliCommand>`, with
   `Create()` returning the second type instead of the shared base. That
   still leaves two things unanswered for an uncovered sibling — which word
   group it falls back under, and which word triggers it — both still
   guessed, same as option 1. Avoiding the guess entirely means a
   hand-written factory for every sub-command, bringing back the exact
   busywork [#289](https://github.com/KitCli/KitCli/issues/289) wants gone.
   Breaking either way: every factory written today takes one type, not two.

## What was established

1. **One word group covers a command and all its sub-commands.** `/hello`,
   `/hello there`, and `/hello mate` are all looked up by "hello"; the
   extra word is checked afterwards, by whichever factory is in charge.
   That's why nothing today tells a covered sub-command apart from an
   uncovered one — every factory in the group shares the same declared
   type, so the difference lives only inside each one's own logic,
   invisible until it runs.
2. **Confirmed the reported failure and the obvious fix's cost.** One
   hand-written factory covering one sub-command leaves every other ask in
   its group — plain command included — crashing outright. Leaving the
   automatic fallback switched on everywhere fixes the crash, but an
   uncovered sub-command stops crashing too — it silently builds the plain
   command instead of its own.

## Evidence

Set up one small family by hand — plain `HelloCliCommand`, sub-commands
`HelloThereCliCommand` and `HelloMateCliCommand` — with a
`HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>` whose
`CanCreateWhen()` checks `SubCommandIs("there")` and whose `Create()`
returns `new HelloThereCliCommand()`. Neither other command needs more than
"create it," so neither got a factory.

Typed `/hello`, `/hello there`, `/hello mate` through KitCli's actual
command-resolution code on `main` (`bbea37e`), then again after patching it
so the automatic fallback is always switched on too:

| Typed | Today | With the fallback always on too |
|---|---|---|
| `/hello` | crashes: "did not find a command factory for hello" | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | crashes, same message as `/hello` | builds `HelloCliCommand` — **wrong command, no crash** |

Option 3 is a design sketch, not yet written or run — nothing has confirmed
it compiles or behaves as described.

## Open questions

- Does this get its own fix, fold into [#188](https://github.com/KitCli/KitCli/issues/188)'s
  larger work, or does option 1 ship first and retire once that work lands?

## Out of scope

- Redesigning how a sub-command's factory shares its word group's name — settled, already relied on elsewhere.
- Sub-commands more than one word long — covered by investigation [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
