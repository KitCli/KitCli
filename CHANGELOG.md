# Changelog

All notable changes to KitCli's packages are documented here. Format is
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). All 9 packages
version together — see `CONTRIBUTING.md#versioning--releases` for why.

## [Unreleased]

### Added

- `[CliCommandAlias("...")]` — declare extra instruction names a `CliCommand`
  responds to, alongside its mechanically-derived full and shorthand
  names, without renaming the type. Repeatable for more than one alias.
  See [0007-cli-command-alias-attribute.md](docs/adr/0007-cli-command-alias-attribute.md).

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
