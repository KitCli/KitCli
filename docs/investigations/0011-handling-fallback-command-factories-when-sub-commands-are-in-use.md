# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity.**

Someone can type a command into a KitCli app, like `/hello`. Some commands
take it further with an extra word right after, like `/hello there` —
asking for one specific thing out of several the plain command could mean.

Behind the scenes, each of these needs a short instruction sheet telling
the app exactly what to build. Most of the time nobody has to write one:
if a command needs nothing beyond "build the plain, empty version,"
KitCli writes that sheet for you — a **default sheet**.

The moment someone writes their own sheet, by hand, for just one
extra-word version, KitCli stops writing default sheets for everything
else that starts with the same word — the plain command included.

The fix that looks obvious — tell KitCli to keep writing default sheets no
matter what — was tried. It stops the crash. But it also makes KitCli
quietly build the wrong thing for any other extra-word version nobody
wrote a sheet for, instead of honestly admitting it doesn't know what to
do. That's worse than the crash it replaces.

## Recommendation

Three candidate fixes, each a real option with a real cost. Pick one
before turning [#289](https://github.com/KitCli/KitCli/issues/289) into
work to build — the third needs its own estimate, not #289's current
"small fix" label.

1. **Guess from the sheet's own label.** Every hand-written sheet in this
   codebase already happens to be named after the exact thing it builds.
   Treat that as proof it's covered, and write default sheets for
   everything else. Cheap, but fragile — rename a sheet, and the app
   silently loses track of what it covers.
2. **Wait for commands to say what they need.** Separate, larger work is
   already planned ([#188](https://github.com/KitCli/KitCli/issues/188)).
   It would have every sheet state plainly, as a fact anyone can check,
   exactly which extra-word version it's for — no guessing. The right
   long-term answer, but it depends on other unfinished work.
3. **Give a hand-written sheet two labels instead of one** — worked
   through below.

### Option 3, worked through

Right now, a hand-written sheet only has room for one label: which
starting word it's for — "hello". It has nowhere to also say which exact
thing it builds. So KitCli can't tell "somebody already wrote a sheet for
`hello there`" apart from "nobody's written one for `hello mate` yet" —
both sheets, if they existed, would carry the identical label.

The fix: give the sheet a second label, so it can say both things. One
label still says which starting word it's for. The other says, plainly,
which exact thing it builds. *(In code: `CliCommandFactory<HelloCliCommand,
HelloThereCliCommand>` instead of today's `CliCommandFactory<HelloCliCommand>`
— the first type stays the shared starting word, the second is new.)*

Now KitCli can check that second label directly, for any extra-word
version, and know for certain whether it's covered — a real check, not a
guess.

That only answers "is it covered?" Say KitCli has worked out that nobody's
written a sheet for `hello mate`. It still needs two more answers before
it can write a default sheet automatically:

- Which starting word to file the default sheet under. It has to be
  "hello" — the same word the plain command and `hello there` already
  use, not something built from `hello mate` on its own, which nobody
  would ever type to look it up.
- Which exact word, typed after "hello", should make that default sheet
  apply. It has to be "mate".

Neither answer comes from the second label. Both still have to be guessed
from the name `hello mate` itself — the same guess option 1 makes.

It's also a breaking change: every sheet written today only has room for
one label, not two.

## What was established

1. **One starting word covers a command and everything that follows it.**
   `/hello`, `/hello there`, and `/hello mate` are all looked up by the
   single word "hello"; the extra word is only checked afterwards, by
   whichever sheet is in charge.
2. **That's why nothing today tells a covered extra-word version apart
   from an uncovered one.** Every sheet sharing a starting word carries
   the identical label. The real difference between them lives only
   inside each sheet's own logic — invisible until it actually runs.
3. **Confirmed the reported crash.** One hand-written sheet covering one
   extra-word version leaves everything else sharing its starting word —
   plain command included — with nothing to build it, and each one
   crashes outright.

## Evidence

Wrote one small family by hand: a plain `HelloCliCommand`, and two
extra-word versions, `HelloThereCliCommand` and `HelloMateCliCommand`.
Wrote a sheet (a factory, in code) for `there` only — neither of the
other two needs more than "build it plain":

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Typed `/hello`, `/hello there`, and `/hello mate` through KitCli's actual
command-resolution code on `main` (`bbea37e`), then again after patching
it so the default sheet always gets written too:

| Typed | Today | With the default sheet always written too |
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
