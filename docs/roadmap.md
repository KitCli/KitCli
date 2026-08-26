# Roadmap

KitCli became its own repository on 28 January 2026, but the code is nearly
a year older. It grew inside a personal finance CLI —
[SpendfulnessCli](https://github.com/joshuaedwardcrowe/SpendfulnessCli) —
as the machinery that app needed to get from a line the user typed to
something that ran, and was lifted out once none of it was about budgets
any more.

Each month names what changed, not every commit.

## 2025 — inside the app it was written for

| Month | What happened |
| --- | --- |
| **February** | The origin. `BaseCommandHandler` arrives on the 5th, two days after the app's first commit, and twelve minutes later `ICommand`, `ICommandGenerator`, `ICommandHandler` and a DI-driven `ConsoleApplication` sit beside it. On the 8th an instruction structure, typed argument builders and a parser turn a typed line into one of those commands. Four of [the seven words](README.md#the-seven-words) exist by the end of the month; outcome, artefact and the run do not. |
| **March – September** | Seven months of no framework work at all. The app built aggregators, a database layer and YNAB clients on top of the command machinery without changing it — which is the evidence the shape was right. |
| **October** | It becomes a framework. "Wrote Cli Abstractions" (23rd) introduces outcomes, the workflow and `CliApp`; "Added Projects" (24th) carves `Cli.Abstractions`, `Cli.Commands`, `Cli.ViewModel.Abstractions` and `Cli.Workflow` out of the app. Types take the `Cli` prefix they still have, view models become `CliTable`, and instruction parsing is rewritten around token indexing. |
| **November** | The vocabulary settles into what is documented today. Generators become [factories](concepts/0001-command-registration.md), command properties become [artefacts](concepts/0008-artefacts.md), and reusable outcomes plus a `ReachedReusableOutcome` status let one run survive several asks — the [workflow state machine](concepts/0010-workflow-run-state-machine.md) in outline. Instruction validators and configurable prefixes land alongside the first real test coverage of the state machine. |
| **December** | Paging: through the list aggregator, and as [outcomes](concepts/0006-outcomes.md) a later command can read back. Artefacts get names. |

## 2026 — its own package

| Month | What happened |
| --- | --- |
| **January** | The split. The app moves to .NET 10, then on the 28th the `Cli.*` projects are copied into a new repository as `KitCli.*`. All nine packages publish at 1.0.0 the next day, and SpendfulnessCli deletes its copies and consumes the package instead. |
| **February** | The first month of being a dependency rather than a folder. Registries and same-assembly auto-registration remove the wiring a consumer had been writing by hand; command reactions and automatic chaining arrive; outcome lists turn fluent and artefacts become records. |
| **March – June** | Quiet. No commits. |
| **July** | Process, not code. `CONTRIBUTING.md`, the first [ADRs](adr/0001-mediatr-for-command-dispatch.md) and [concept docs](concepts/0001-command-registration.md), and an automated release workflow — the paper trail the framework had been running without. |
| **August** | The busiest month by a distance. A [headless host](adr/0013-merge-the-hosts-and-name-the-variant-headless.md) runs a command straight from process args; Ctrl+C becomes [cooperative cancellation](adr/0006-cooperative-cancellation.md); every run gets [its own DI scope](adr/0002-di-scope-per-workflow-run.md). `[CliCommandAlias]` and `[CliNextCommandIs]` let a command declare its extra names and what should follow it, and a chain can [name the next command by type](adr/0011-chain-to-a-command-by-type.md). The release tool is rebuilt as a KitCli app, so publishing the packages now uses them. Five days at the end of the month carry v1.0.11 through to v3.0.0, and these docs go up as a site. |
