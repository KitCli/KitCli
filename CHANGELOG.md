# Changelog

All notable changes to KitCli's packages are documented here. Format is
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Each of the 9
packages has its own version, and only what changed gets a new one — see
`CONTRIBUTING.md#versioning--releases`.

## [Unreleased]

## [3.4.0] - 2026-10-15

### Added

- An outcome can last the whole session: pass `OutcomeLifetime.Session` to
  `Outcome`'s constructor and every later run reads it ([#311](https://github.com/KitCli/KitCli/issues/311)).
- `ByPausing()` writes a blank line between a command's messages
  ([#106](https://github.com/KitCli/KitCli/issues/106)).
- `LastCommandIsOneOf<A, B>()` checks whether the last command was any of up to
  five types ([#330](https://github.com/KitCli/KitCli/issues/330)).
- A command can read `CanAsk` on its `ICliIo` to skip a prompt in a headless app,
  where prompting throws `NoInputException` (the [CanAsk ADR](docs/adr/let-a-command-ask-its-io-whether-anyone-can-answer.md)).

### Changed

- A run's status history now starts with a `Created → Created` change holding
  the session's kept outcomes, so code reading the first change sees that first.

### Deprecated

- `AddCliAbstractions()`, because `CliAppBuilder` now registers the `ICliIo`
  itself. Delete the call.

### Fixed

- An anonymous outcome such as `BySaying` no longer ends the run when it comes
  last. The run decides on the last outcome that is not anonymous
  ([#297](https://github.com/KitCli/KitCli/issues/297)).

## [3.3.0] - 2026-10-15

### Added

- `ICliWorkflowRunState.WasLastChangedTo(...)` checks the run's latest status
  change, where `WasChangedTo(...)` checks its whole history. An app with its
  own `ICliWorkflowRunState` implementation needs to add it.
- `ByMovingToCommand(Type)` moves to a command whose type is only known at run
  time ([#314](https://github.com/KitCli/KitCli/issues/314)).
- `IsSubCommand<TCommand>()` on a command factory checks the sub-command by
  command type instead of a name string.
- `cliIo.SayCommandSuggestion(name, description)` writes a suggested command in
  the same indented block a suggestion outcome uses
  ([#313](https://github.com/KitCli/KitCli/issues/313)).
- `ICliIo.Say(message, level)` writes a message only when its `MessageLevel`
  meets `KitCliSettings.MinimumMessageLevel`, the same rule `BySaying(message,
  level)` follows. An app with its own `ICliIo` implementation needs to add it
  ([#310](https://github.com/KitCli/KitCli/issues/310)).
- `LastCommandIs<T>()` on a command factory is true only when `T` ran
  immediately before. `LastCommandWas<T>()` is true when `T` ran at any earlier
  point in the run, so two factories gated on different callers could both
  match; gate on `LastCommandIs<T>()` to tell the callers apart.
  `AnyLastCommandIs<T>()` is the explicit name for the any-earlier-run check
  ([#321](https://github.com/KitCli/KitCli/issues/321)).

### Deprecated

- `LastCommandWas<T>()` on a command factory is obsolete. Despite its name it
  matches any earlier run of `T`; use `AnyLastCommandIs<T>()` for that, or
  `LastCommandIs<T>()` for the command that ran immediately before
  ([#321](https://github.com/KitCli/KitCli/issues/321)).

### Fixed

- A command's `[CliNextCommandCouldBe]` suggestions no longer print when it
  moves straight on to another command with `ByMovingToCommand`. They print
  when the run parks or finishes
  ([#315](https://github.com/KitCli/KitCli/issues/315)).

### Changed

- **Breaking for `KitCli.Commands.Abstractions` users:** `MessageLevel` and
  `KitCliSettings` moved to `KitCli.Abstractions.Io` so `ICliIo` can use them.
  Add `using KitCli.Abstractions.Io;` where they are named. Code using only the
  `KitCli` package is unaffected.

## [3.2.0] - 2026-10-11

### Added

- `BySaying(message, level)` and `ByFinallySaying(message, level)` give a
  message a `MessageLevel` (`Trace` through `Error`), shown only when it
  meets `KitCliSettings.MinimumMessageLevel` (defaults to `Information`).
  Configure it under a `KitCli` section, loaded via `WithJsonSettings`/
  `WithUserSecretSettings`, same as any other KitCli-owned setting. The
  existing `BySaying(message)` is unaffected — a plain message always shows
  ([#299](https://github.com/KitCli/KitCli/issues/299)).
- `[CliNextCommandCouldBe(name, description)]` suggests commands once a
  command finishes, even outside a parked run — `[CliNextCommandIs]` only
  ever fired when a parked run's next ask failed to resolve
  ([#294](https://github.com/KitCli/KitCli/issues/294)).
- `ICliIo.ChooseAsync` and `ICliIo.ChooseManyAsync` show a list the person
  moves through with the arrow keys, picking one item with enter or ticking
  several with the space bar. Each item is a `CliChoice` — a `Label` to show
  and an optional `Value` handed back with the pick. An app with its own
  `ICliIo` implementation needs to add both
  ([#255](https://github.com/KitCli/KitCli/issues/255)).

### Changed

- `CliApp.OnRunStarted` now runs just before the command starts instead of
  just after, so a line it writes no longer lands on top of a list the
  command shows.
- A suggested-next-command now prints under a header line instead of bare,
  with its name and description indented on separate lines instead of flush
  left.

### Deprecated

- `CliApp.OnRunStarted` now runs straight after `OnRunCreated`, so it tells
  an app nothing that hook does not. Override `OnRunCreated` instead; removal
  is tracked in [#305](https://github.com/KitCli/KitCli/issues/305).

### Fixed

- An app whose entry project has no commands now starts when a registry adds
  them from another project ([#118](https://github.com/KitCli/KitCli/issues/118)).

## [3.1.2] - 2026-09-18

### Fixed

- `ByMovingToCommand<T>()` no longer prints a blank line for every command it
  chains through. One now prints after the whole chain, same as any other
  command ([#290](https://github.com/KitCli/KitCli/issues/290)).

## [3.1.1] - 2026-09-16

### Added

- `Table.Style`, and `TableBuilder.WithStyle(...)`, choose how a table's lines
  are drawn, from nineteen `CliTableStyle` values. `Ascii` is the box every
  table has drawn until now and stays the default. `None` draws nothing and
  lines the columns up by spacing alone, `HeaderLine` draws only a rule under
  the column headers, and `Markdown` writes a table that renders as one when
  pasted into an issue or a document. Lines between rows travel with the
  choice: only the styles that draw a box draw them
  ([#263](https://github.com/KitCli/KitCli/issues/263)).
- `ByReacting<TReaction>()` names a reaction by type instead of building it, the
  way `ByMovingToCommand<TCommand>()` names the next command. The run constructs
  it through a `CliCommandReactionFactory<TReaction>` — auto-registered for
  parameterless reactions — so the factory sees the run's artefacts
  ([#204](https://github.com/KitCli/KitCli/issues/204)).

### Fixed

- `AddCommandsFromAssembly` no longer throws at startup when a command type has
  more than one `CliCommandFactory<T>`. Each is registered, and the first
  whose `CanCreateWhen()` returns `true` builds the command, same as before
  the regression ([#283](https://github.com/KitCli/KitCli/issues/283)).
- An argument value can now contain `--`, as in `--range 10--20` or
  `--note see -- there`. Until now any `--` anywhere in the line started a new
  argument and cut the value short. A `--` written directly in front of a word
  still starts an argument, which is what `--name` has always meant
  ([#39](https://github.com/KitCli/KitCli/issues/39)).
- A table cell holding line breaks no longer breaks the box around it. Its
  lines now render inside the cell's own row
  ([#214](https://github.com/KitCli/KitCli/issues/214)).

### Changed

- Tables render through Spectre.Console instead of ConsoleTables, so every
  table looks slightly different. A table is now sized to fit the console it is
  printed to, so a long value wraps inside its own column instead of running off
  the side. `Table.MaxColumnWidth` keeps its name, and now holds a column
  narrower than that. See
  the [Spectre.Console ADR](docs/adr/render-tables-with-spectre-console.md).

## [3.1.0] - 2026-08-27

### Added

- `Table.MaxColumnWidth`, and `TableBuilder.WithMaxColumnWidth(...)`, set the width a
  column's text may reach before a cell is broken across lines. Unset, no cell
  breaks.

### Fixed

- A table cell longer than 40 characters is no longer chopped in half and ruled
  off as two rows ([#205](https://github.com/KitCli/KitCli/issues/205)).
- Typing something that isn't a command, mid-run, no longer ends the app — the
  run keeps its place and suggests what would work
  ([#182](https://github.com/KitCli/KitCli/issues/182)).

## [3.0.0] - 2026-08-25

### Fixed

- A headless (formerly args) invocation now runs a chained command's every
  step, instead of running the first and exiting 0
  ([#167](https://github.com/KitCli/KitCli/issues/167)). `ExecuteRunOperation`
  drives the run past each queued step, so a chain arrives whole under
  either host.
- A run that reached a reusable checkpoint no longer crashes with
  `ImpossibleStateChangeException` when the input ends. A session with no
  ask left ends instead of responding to a null one.

### Changed

- **Breaking:** `TerminalCliApp` is gone — `CliApp` is the interactive app.
  Subclass it directly.
- **Breaking:** `ArgsCliApp` is now `HeadlessCliApp`, `BasicTerminalCliApp`
  is `BasicCliApp`, and `CliAppBuilder.WithBasicTerminalApp()` is
  `WithBasicApp()`. See
  [merge-the-hosts-and-name-the-variant-headless.md](docs/adr/merge-the-hosts-and-name-the-variant-headless.md).
- A headless app rethrows an `ExceptionOutcome` as an interactive one
  always has, so a throwing handler ends the process non-zero rather than
  at 0.

## [2.0.0] - 2026-08-25

### Added

- `ByMovingToCommand<TCommand>(arguments)` — chain by naming the next command's
  type; its factory builds it from the run's artefacts and any
  `NextCliCommandArgument<TValue>`s you pass.
  See [chain-to-a-command-by-type.md](docs/adr/chain-to-a-command-by-type.md).
- `CliCommand.GetInstructionName(Type)` — a command's instruction name,
  without needing an instance of it.

### Changed

- **Breaking:** `NextCliCommandOutcome` is now a base type — construct
  `ProvidedNextCliCommandOutcome` instead. Matching on it is unaffected.

## [1.0.13] - 2026-08-24

### Fixed

- `CliAppBuilder.Run` built its service provider with no validation, so a
  singleton that depended on a `Scoped` service — an `IOutcomeIoWriter`
  taking a scoped collaborator, for instance — was silently allowed. The
  singleton captured whichever instance existed when it was first resolved
  and held it for the app's whole lifetime, quietly diverging from the
  fresh per-run instance every command handler gets. Nothing failed or
  warned. The provider is now built with `ValidateScopes` and
  `ValidateOnBuild` on, so this fails at startup, naming the singleton and
  the scoped service it can't consume. See
  [0002-cli-app-host.md](docs/concepts/0002-cli-app-host.md).

## [1.0.12] - 2026-08-23

### Added

- `[CliCommandAlias("...")]` — declare extra instruction names a `CliCommand`
  responds to, alongside its mechanically-derived full and shorthand
  names, without renaming the type. Repeatable for more than one alias.
  See [cli-command-alias-attribute.md](docs/adr/cli-command-alias-attribute.md).
- `[CliNextCommandIs("...", "...")]` — declare the instruction name(s) and
  description(s) a `CliCommand` expects next once it reaches a reusable
  outcome. When a later ask doesn't resolve to any command, the workflow
  run now suggests these instead of returning silently. Repeatable for more
  than one next command.
  See [suggest-next-commands-attribute.md](docs/adr/suggest-next-commands-attribute.md).

### Fixed

- A workflow run that ended via a mistyped/empty ask, an unrecognized
  command, or a failed command handler could be left one status short of
  `Finished`. `CliWorkflow.NextRun()` then handed that run back for the
  next ask instead of starting a new one, crashing with
  `ImpossibleStateChangeException` on the following input.
  `NextRun()` now checks `Finished` (not just `ReachedFinalOutcome`)
  before reusing a run, and every terminal path drives the run all the
  way to `Finished` before returning. See
  [0010-workflow-run-state-machine.md](docs/concepts/0010-workflow-run-state-machine.md).

### Changed

- A command handler that throws an unhandled exception now ends the whole
  interactive session instead of silently continuing to the next ask. See
  [0002-cli-app-host.md](docs/concepts/0002-cli-app-host.md).

## [1.0.11] - 2026-08-22

### Added

- `ArgsCliApp` — a `CliApp` base for running a single command from process
  args in one shot (`myapp /command --flag value`), instead of only through
  an interactive prompt. `TerminalCliApp` carries the pre-existing
  interactive loop under its own name. See
  [args-driven-cli-app.md](docs/adr/args-driven-cli-app.md).

### Changed

- **Breaking:** `BasicCliApp`/`CliAppBuilder.WithBasicCli()` renamed to
  `BasicTerminalCliApp`/`WithBasicTerminalApp()`;
  `CliAppBuilder.WithCli<TCliApp>()` renamed to `WithApp<TCliApp>()`. (#81)
- **Breaking:** `CliApp.Run` no longer exists on the base class — it now
  lives on `TerminalCliApp`/`ArgsCliApp` depending on which an app extends.
  `CliAppBuilder.Run()` dispatches to the right one automatically. (#81)

### Fixed

- `CliApp`/`CliAppBuilder` now create a DI scope per workflow run, so
  `Scoped`-registered services get a fresh instance per command instead
  of behaving like singletons for the process lifetime. (#71)
- **Breaking:** Ctrl+C no longer calls `Environment.Exit` from a background
  thread mid-run, which could dispose a run's DI scope out from under it
  and skip pending `finally`/`Dispose()` cleanup. Cancellation is now
  cooperative: `ICliIo.Ask()` is now `AskAsync(CancellationToken)`, and
  `ICliWorkflow` gained `CancellationToken` and `InterruptCurrentRun()`, so
  an in-flight run can unwind through its own `catch`/`finally` instead of
  being killed out from under it. See
  [cooperative-cancellation.md](docs/adr/cooperative-cancellation.md). (#74)
