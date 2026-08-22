# Changelog

All notable changes to KitCli's packages are documented here. Format is
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). All 9 packages
version together — see `CONTRIBUTING.md#versioning--releases` for why.

## [Unreleased]

### Fixed

- `CliApp`/`CliAppBuilder` now create a DI scope per workflow run, so
  `Scoped`-registered services get a fresh instance per command instead
  of behaving like singletons for the process lifetime. (#71)
