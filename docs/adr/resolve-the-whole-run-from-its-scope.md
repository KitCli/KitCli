# 0010. Resolve the whole run from its scope

Status: Proposed
Date: 2026-08-24

Supersedes [0002](0002-di-scope-per-workflow-run.md).

## Context

[ADR 0002](0002-di-scope-per-workflow-run.md) created one DI scope per run,
and decided `ICliWorkflowCommandProvider` stays `Singleton` because it only
resolves other singletons. That reasoning holds today, and is circular:
those factories are singletons because nothing made them otherwise.

The consequence 0002 did not state is that **a command handler is the only
extension point that can reach run-scoped state.** Two mechanisms cause it.

`CliWorkflowCommandProvider` is a singleton injecting `IServiceProvider`.
Singletons are constructed in the root, so the provider it captures is the
root provider — even though `CreateNewRun()` resolves it *from* the run's
scope. Factories resolving through it never see that scope. MediatR's
`ISender` is transient, constructed inside the scope, which is exactly why
handlers do see it.

Second, the run's scope is disposed inside `ExecuteCommand`'s `finally` —
before `CliApp.WriteOutcomes` runs. A writer resolving from the scope gets
`ObjectDisposedException` when the command ended the run, and succeeds when
it did not: correct in testing, broken on the last step.

Neither is caught by `ValidateScopes`/`ValidateOnBuild`. See
[microsoft-dependency-injection.md](../technology/microsoft-dependency-injection.md).

## Decision

The scope stays per *run* — a run is the unit of work, not a command. What
changes is that the whole run resolves from it, end to end, the way an
ASP.NET Core request scope wraps writing the response too.

**Every KitCli service that executes during a run is registered `Transient`
and resolved from the run's scope.** Only services that genuinely outlive a
run stay `Singleton`: `ICliIo`, `ICliWorkflow`, and `CliApp`.

`Transient` rather than `Scoped`, because the lifetime KitCli registers
constrains what a consumer can inject into their own implementation:

| Extension point registered | consumer's `Scoped` dep | consumer's `Transient` dep |
|---|---|---|
| `Scoped` | honoured | captured for the whole run |
| `Transient` | honoured | honoured |

`Transient` is a strict superset. MediatR's handlers are already registered
this way, and are the one part of the framework that honours a consumer's
lifetimes today.

The rule reaches the instruction pipeline, `ICliWorkflowCommandProvider`,
`ICliCommandFactory` (as keyed-`Transient`), `IArtefactFactory`, and
`IOutcomeIoWriter` — the last resolved **per run from the run's scope**
instead of once from the root, the shape ASP.NET Core uses for
factory-activated middleware. `Write`'s signature is unchanged. The scope
is therefore disposed **after** its outcomes are written:
`UpdateStateWhenFinished` still marks `Finished`; the host owns disposal.

The point is not that KitCli's own types need a particular lifetime. It is
that a consumer's registration is honoured wherever their code runs,
instead of being silently promoted to process lifetime by a root-resolved
holder.

## Alternatives considered

- **Keep 0002 and document the limit** — free and honest, but the framework
  advertises a per-run scope only one extension point can use, which is the
  more expensive kind of surprise.
- **A scope per command** — rejected in 0002 and still rejected: it breaks
  any multi-turn run expecting a scoped service to survive between steps.
- **Leave writers alone; tell consumers to use `IDbContextFactory<T>`** —
  solves one library's case, and still hands the writer a different
  instance from the one its own handler used.
- **Register run participants `Scoped`** — the obvious reading, and it
  honours only one of the two lifetimes a consumer can register.
- **Give `Write` a context object carrying the run's `IServiceProvider`** —
  a service locator, breaking every writer's signature. ASP.NET Core treats
  `RequestServices` as the escape hatch for when per-request activation is
  impossible. Here it is not.

## Consequences

- `IOutcomeIoWriter.Write` keeps its signature, so no custom writer breaks.
  Writers are constructed once per run instead of once per process, so a
  writer holding state across runs changes behaviour.
- `ICliWorkflowCommandProvider` changing lifetime breaks anyone resolving
  it from the root provider directly.
- Disposal moves out of `CliWorkflowRun` into the host, so `ICliWorkflowRun`
  exposes its scope. The trigger is "the write that followed the command
  that finished the run", not every write.
- **Consumer artefact factories still cannot take constructor
  dependencies.** They are built by `Activator.CreateInstance`, not by DI.
  This ADR knowingly leaves that unfixed — a separate defect in how those
  factories are registered, not a lifetime decision.
