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

Five tickets. The first two stand alone and can go now.

1. **A wrong command prints nothing** — `bug`, `area:workflow`. Say
   *Unknown command. Please use /help to see all available commands.* Split
   the two silences first: a line naming no command, and a name matching none.
2. **A table can drop its borders** — *Print More Than Plain Text*. Help
   listings are drawn without rules or boxes, and
   [#260](https://github.com/KitCli/KitCli/issues/260) covers only columns.
3. **A command carries a description**, written into
   [#191](https://github.com/KitCli/KitCli/issues/191)'s descriptor rather
   than added as a fourth attribute beside the three already there.
4. **Registration keeps a list of what it registered.** The scan already
   walks every command and works out its names. Writing them down is the
   cheap half of [#190](https://github.com/KitCli/KitCli/issues/190).
5. **`/help` itself** — three files beside `Exit`, printing a borderless
   table. A second permutation takes a command name and shows that one
   command in full, which needs #190's argument declarations.

## What was established

1. **The name lookup only runs one way.** Every command is filed under its
   full name, its shorthand and each alias. Ask it for `spare-money` and it
   answers. Ask what names it holds and it cannot: .NET has no way to read
   the keys back out once the app has started.
2. **Nothing holds a description, and `///` comments cannot become one.**
   A command's name is worked out from its type name and nothing else is
   kept. The C# compiler never puts documentation comments into the built
   assembly; they go to a separate file next to it, which every app would
   have to switch on and ship. Spectre.Console, McMaster and
   CommandLineParser all take the text from an attribute instead.
3. **[#188](https://github.com/KitCli/KitCli/issues/188) already says
   this.** Its list of consequences reads "there is no `--help`, no
   catalogue". The descriptor it proposes is built while the app starts and
   readable without constructing anything, which is the missing list.
4. **A wrong command prints nothing at all.** The run marks the ask invalid
   and returns an outcome that no writer claims. A "Command Not Found"
   outcome and its writer already exist, and the only thing producing them
   is a playground handler.
5. **Two different failures share that silence.** No command registered
   under the name is a wrong command. A command that is registered but
   declines to run is not, and sending that one to `/help` points the
   reader at an entry sitting in the list already. Telling them apart is
   [#219](https://github.com/KitCli/KitCli/issues/219).
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

The run knew the ask was bad and finished quietly. A real app shows
nothing and asks again.

## Open questions

- One row per command, or one per name? Every command answers to a full
  name and a shorthand, and some to aliases too.
- What order, and how is a built-in marked? There are no categories to
  group by.
- What fills the description column for a command whose author wrote none?

## Out of scope

- **Argument and flag help.** Nothing declares arguments yet. That is
  [#190](https://github.com/KitCli/KitCli/issues/190).
- **Whether an outcome with no writer should be silent.** Already
  [#18](https://github.com/KitCli/KitCli/issues/18).
