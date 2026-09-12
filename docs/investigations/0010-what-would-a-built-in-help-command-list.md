# 0010. What would a built-in /help command list?

- **Status:** In Review
- **Spike:** not yet filed
- **Time-box:** none agreed — ran inline
- **Date:** 2026-09-12

## Verdict

**New complexity, and none of it is in the command.** Shipping a `/help` to
every app built on KitCli is a solved shape. `/exit` is already one.

The problem is that KitCli cannot answer the question `/help` asks. An app
has no way to list its own commands, and no command holds a sentence saying
what it does. Both gaps are what *Describing a Command* exists to close, so
`/help` waits on that milestone and reads what it produces.

## Recommendation

Five tickets, filed. Only the second starts unblocked.

1. [#262](https://github.com/KitCli/KitCli/issues/262) **A wrong command
   prints nothing.** Say *Unknown command. Please use /help to see all
   available commands.* Blocked by
   [#219](https://github.com/KitCli/KitCli/issues/219), which splits the two
   reasons a name fails.
2. [#263](https://github.com/KitCli/KitCli/issues/263) **A table can drop its
   borders.** Help listings are drawn without rules or boxes, and
   [#260](https://github.com/KitCli/KitCli/issues/260) covers only columns.
3. [#264](https://github.com/KitCli/KitCli/issues/264) **A command carries a
   description**, written into
   [#191](https://github.com/KitCli/KitCli/issues/191)'s descriptor rather
   than added as a fourth label beside the three already there.
4. [#265](https://github.com/KitCli/KitCli/issues/265) **Registration keeps a
   list of what it registered**, which the scan already walks. The cheap half
   of [#190](https://github.com/KitCli/KitCli/issues/190).
5. [#266](https://github.com/KitCli/KitCli/issues/266) **`/help` itself** —
   three files beside `Exit`, printing a borderless table. A second
   permutation takes a command name and shows that one in full, which needs
   #190's argument declarations.

The first two sit in *Print More Than Plain Text*, with #219 moved there as
the enabler. The last three hang off #188 in *Describing a Command*.

## What was established

1. **The name lookup only runs one way.** Every command is filed under its
   full name, shorthand and aliases. Ask for `spare-money` and it answers;
   ask what names it holds and it cannot, once the app has started.
2. **Nothing holds a description.** A command's name comes from its type
   name, and nothing else is kept. Documentation comments cannot fill the
   gap: they never reach the finished program, only a separate file each app
   would have to switch on and ship. Other .NET libraries all ask the author
   to write the sentence somewhere the program carries.
3. **[#188](https://github.com/KitCli/KitCli/issues/188) already says
   this.** Its list of consequences reads "there is no `--help`, no
   catalogue". The descriptor it proposes is built while the app starts and
   readable without constructing anything, which is the missing list.
4. **A wrong command prints nothing at all.** The run marks the ask invalid
   and returns an outcome that no writer claims. A "Command Not Found"
   outcome and its writer already exist, and the only thing producing them
   is a playground handler.
5. **Two different failures share that silence.** No command under the name
   is a wrong command. A command that exists but declines to run is not, and
   `/help` would send that reader to an entry already in the list.
6. **The table frame is fixed in code.** Every table draws with an ASCII
   border and a line between rows, set where the table renders itself and
   exposed nowhere. Spectre.Console can draw no border at all.
7. **Offering a near match is what other tools do.** git, npm and Cobra name
   the closest command they know to what was typed, Cobra at an edit
   distance of two. Each needs the list first.

## Evidence

Everything below is the playground's own lifecycle messages. Run a wrong
command against `5123d0e`:

```
dotnet run --project KitCli.Playground.App.Headless -- "/nonsense"

TestCliApp run completed.
Run state changes: Running, InvalidAsk, Finished
Run outcomes achieved: NothingOutcome
```

The run knew the ask was bad and finished quietly. A real app shows nothing.

## Open questions

- Three shape questions, carried on #266: one row per command or one per
  name, what order and how a built-in is marked, and what an empty
  description column shows.

## Out of scope

- **Argument and flag help.** Nothing declares arguments yet. That is
  [#190](https://github.com/KitCli/KitCli/issues/190).
- **Whether an outcome with no writer should be silent.** Already
  [#18](https://github.com/KitCli/KitCli/issues/18).
