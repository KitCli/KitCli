# 0002. Create a DI scope per workflow run

Status: Accepted
Date: 2026-08-22

[ADR 0010](0010-resolve-the-whole-run-from-its-scope.md) proposes to
supersede this. It keeps the per-run scope and extends it to the whole run;
read it before relying on the lifetimes decided here.

## Context

`CliAppBuilder.Run()` called `_services.BuildServiceProvider()` once and
resolved `CliApp` (and everything it depends on) directly from that root
provider. `CliWorkflow.CreateNewRun()` did the same for each run's
`IInstructionParser`, `IInstructionValidator`, `ICliWorkflowCommandProvider`,
`ISender`, and `IPublisher`. No scope was ever created anywhere in the
run loop (confirmed by a repo-wide search for `CreateScope`/`IServiceScope`/
`ValidateScopes` turning up nothing), and `ValidateScopes` defaults to
`false`, so nothing caught the misuse.

The practical effect: a consumer registering a `Scoped` service — the
documented default lifetime for `AddDbContext<T>()`, for example — got a
single instance living for the whole process instead of one per unit of
work, silently. This surfaced as issue #71.

## Decision

`CliWorkflow` now depends on `IServiceScopeFactory` instead of
`IServiceProvider`. `CreateNewRun()` creates one `IServiceScope` per run
and resolves that run's `IInstructionParser`, `IInstructionValidator`,
`ICliWorkflowCommandProvider`, `ISender`, and `IPublisher` from
`scope.ServiceProvider`. `CliWorkflowRun` holds that scope and disposes it
in `UpdateStateWhenFinished()`, the moment the run's state reaches
`Finished` — see
[0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md)
for how that guard fires exactly once per run.

This is a per-*run* scope, not a per-*command* scope: a multi-turn run
(one that loops through `ReachedReusableOutcome` or `MovePastAsk`) keeps
the same scope, and so the same `Scoped` instances, across every command
in that run. That mirrors ASP.NET Core's per-request scope in spirit —
one unit-of-work boundary — but the unit of work here is the whole run,
not a single command inside it.

`ICliWorkflowCommandProvider` itself stays registered `Singleton`: it only
resolves the (also singleton) `ICliCommandFactory`/`IArtefactFactory`
implementations, so it doesn't need to participate in the per-run scope.
The actual per-run isolation guarantee comes from `ISender`/`IPublisher`
being resolved from the run's scope, since that's what MediatR uses to
resolve `IRequestHandler<T>` instances — and their constructor-injected
dependencies — at `Send()` time. That reasoning is what ADR 0010 revisits.

## Alternatives considered

- **`BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true })`**
  — would turn the misuse into a loud failure instead of a silent one, but
  doesn't give consumers correct `Scoped` semantics; it only stops them
  from registering `Scoped` services at all without a scope existing
  somewhere.
- **A scope per command instead of per run** — would give the tightest
  possible isolation, but breaks any multi-turn run that expects a
  `Scoped` service (e.g. a `DbContext` tracking entities across a paging
  interaction) to survive from one command to the next within the same
  run.

## Consequences

`CliWorkflow`'s public constructor signature changed from
`IServiceProvider` to `IServiceScopeFactory` — a breaking change for any
direct consumer of that constructor. Any `Scoped`-registered service now
gets a fresh instance per workflow run instead of behaving like a
singleton for the process lifetime, matching what `AddDbContext<T>()` and
similar registrations already assume.
