# Changelog

All notable changes to KitCli's packages are documented here. Format is
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). All 9 packages
version together — see `CONTRIBUTING.md#versioning--releases` for why.

## [Unreleased]

### Added

- `ByMovingToCommand<TCommand>(arguments)` — chain by naming the next command's
  type; its factory builds it from the run's artefacts and any arguments you pass.
  See [0011-chain-to-a-command-by-type.md](docs/adr/0011-chain-to-a-command-by-type.md).
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
  [cli-app-host.md](docs/concepts/cli-app-host.md).

## [1.0.12] - 2026-08-23

### Added

- `[CliCommandAlias("...")]` — declare extra instruction names a `CliCommand`
  responds to, alongside its mechanically-derived full and shorthand
  names, without renaming the type. Repeatable for more than one alias.
  See [0007-cli-command-alias-attribute.md](docs/adr/0007-cli-command-alias-attribute.md).
- `[CliNextCommandIs("...", "...")]` — declare the instruction name(s) and
  description(s) a `CliCommand` expects next once it reaches a reusable
  outcome. When a later ask doesn't resolve to any command, the workflow
  run now suggests these instead of returning silently. Repeatable for more
  than one next command.
  See [0008-suggest-next-commands-attribute.md](docs/adr/0008-suggest-next-commands-attribute.md).

### Fixed

- A workflow run that ended via a mistyped/empty ask, an unrecognized
  command, or a failed command handler could be left one status short of
  `Finished`. `CliWorkflow.NextRun()` then handed that run back for the
  next ask instead of starting a new one, crashing with
  `ImpossibleStateChangeException` on the following input.
  `NextRun()` now checks `Finished` (not just `ReachedFinalOutcome`)
  before reusing a run, and every terminal path drives the run all the
  way to `Finished` before returning. See
  [workflow-run-state-machine.md](docs/concepts/workflow-run-state-machine.md).

### Changed

- A command handler that throws an unhandled exception now ends the whole
  interactive session instead of silently continuing to the next ask. See
  [cli-app-host.md](docs/concepts/cli-app-host.md).

## [1.0.11] - 2026-08-22

### Added

- `ArgsCliApp` — a `CliApp` base for running a single command from process
  args in one shot (`myapp /command --flag value`), instead of only through
  an interactive prompt. `TerminalCliApp` carries the pre-existing
  interactive loop under its own name. See
  [0005-args-driven-cli-app.md](docs/adr/0005-args-driven-cli-app.md).

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
  [0006-cooperative-cancellation.md](docs/adr/0006-cooperative-cancellation.md). (#74)
