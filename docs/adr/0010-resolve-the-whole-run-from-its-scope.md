# 0010. Resolve the whole run from its scope

Status: Proposed
Date: 2026-08-24

Supersedes [0002](0002-di-scope-per-workflow-run.md).

## Context

[ADR 0002](0002-di-scope-per-workflow-run.md) created one DI scope per
workflow run. It also decided `ICliWorkflowCommandProvider` stays
registered `Singleton`, because it "only resolves the (also singleton)
`ICliCommandFactory`/`IArtefactFactory` implementations, so it doesn't
need to participate in the per-run scope."

That reasoning holds today, and is circular: those factories are
singletons because nothing made them otherwise. The consequence ADR 0002
did not state is that **a command handler is the only extension point that
can reach run-scoped state**. Two mechanisms cause it.

`CliWorkflowCommandProvider` is a singleton that injects
`IServiceProvider`. Singletons are constructed in the root, so the
provider it captures is the root provider — even though `CreateNewRun()`
resolves the provider itself *from* the run's scope. Command and artefact
factories resolve through it and never see that scope. MediatR's `ISender`
is transient, constructed inside the scope, which is exactly why handlers
do see it.

The run's scope is then disposed in
`CliWorkflowRun.UpdateStateWhenFinished()`, inside `ExecuteCommand`'s
`finally` — before `CliApp.WriteOutcomes` runs. A writer resolving from
the scope gets `ObjectDisposedException` when the command ended the run,
and succeeds when it did not: correct in testing, broken on the last step.

Neither mechanism is caught by `ValidateScopes`/`ValidateOnBuild`.
Injecting `IServiceProvider` is legal at every lifetime, and instance
registrations have no call site to validate. See
[microsoft-dependency-injection.md](../technology/microsoft-dependency-injection.md).

## Decision

The scope stays per *run* — a run is the unit of work, not a command. What
changes is that the whole run resolves from it, end to end, the way an
ASP.NET Core request scope wraps the entire request including writing the
response.

**Every KitCli service that executes during a run is registered
`Transient` and resolved from the run's scope.** Only services that
genuinely outlive a run stay `Singleton`: `ICliIo`, `ICliWorkflow`, and
`CliApp`, all of which the host loop uses between runs.

`Transient` rather than `Scoped`, because these are extension points a
consumer implements, and the lifetime we register constrains what they can
inject into their implementation:

| Extension point registered | consumer's `Scoped` dep | consumer's `Transient` dep |
|---|---|---|
| `Scoped` | honoured | captured for the whole run |
| `Transient` | honoured | honoured |

`Transient` is a strict superset — there is no case where `Scoped` is more
correct, only cases where it silently removes an option. MediatR's
`Mediator` and its handlers are already registered this way, and are the
one part of the framework that resolves a consumer's lifetimes correctly
today.

That rule reaches:

- The instruction pipeline — `IInstructionParser`,
  `InstructionTokenIndexer`, `InstructionTokenExtractor`,
  `IInstructionArgumentBuilder`, `IInstructionValidator`.
- `ICliWorkflowCommandProvider`. `CreateNewRun()` already resolves it from
  the scope, so changing its registration is the whole change.
- `ICliCommandFactory`, as keyed-`Transient`.
- `IArtefactFactory`.
- `IOutcomeIoWriter`, resolved **per run from the run's scope** instead of
  once from the root provider in `CliAppBuilder.Run` — the shape ASP.NET
  Core uses for factory-activated middleware
  (`IMiddleware`/`IMiddlewareFactory`). `Write`'s signature is unchanged; a
  writer constructor-injects a scoped service like any other participant,
  because its holder is now per-run.

The run's scope is therefore disposed **after** its outcomes are written.
`UpdateStateWhenFinished` still marks `Finished`; the host owns disposal.

The registration only changes behaviour for `ICliCommandFactory` and
`IArtefactFactory`, which are re-resolved per command. The rest are
resolved once per run and held by `CliWorkflowRun`, so their dependencies
are captured for the run whatever they are registered as — `Transient`
there is for one rule rather than three.

The point is not that KitCli's own types need a particular lifetime. It is
that a consumer's registration is honoured wherever their code runs —
`Scoped` per run, `Transient` per resolution — instead of being silently
promoted to process lifetime by a root-resolved holder.

## Alternatives considered

- **Keep 0002 and document the limit.** Free, and honest. Rejected
  because the framework advertises a per-run scope that only one
  extension point can use, which is the more expensive kind of surprise.
- **A scope per command.** Rejected in 0002 and still rejected: it breaks
  any multi-turn run expecting a scoped service to survive from one
  command to the next.
- **Leave writers alone; tell consumers to use `IDbContextFactory<T>`.**
  Works for EF specifically, and a factory registered `Singleton` does
  resolve. Rejected because it solves one library's case rather than the
  general one, and still gives the writer a different instance from the
  one its own run's handler used.
- **Register run participants `Scoped` rather than `Transient`.** The
  obvious reading of "the run is scoped, so its parts are scoped".
  Rejected: a consumer's `Transient` registration injected into a `Scoped`
  extension point is captured for the whole run, silently, with no
  validation error. `Scoped` would honour one of the two lifetimes a
  consumer can register; `Transient` honours both.
- **Give `Write` a context object carrying the run's `IServiceProvider`.**
  The `HttpContext.RequestServices` equivalent. Rejected: it is a service
  locator, it breaks the signature of every writer including the eight
  built-ins, and ASP.NET Core treats `RequestServices` as the escape hatch
  for cases where per-request activation is impossible. Here it is not —
  moving the resolution site is both smaller and more honest.

## Consequences

`IOutcomeIoWriter.Write` keeps its signature, so no custom writer breaks.
Writers are constructed once per run instead of once per process, so any
writer holding state across runs changes behaviour. `CliAppBuilder.Run`
stops resolving them, and `CliApp.WriteOutcomes` resolves them from the
run — which means `ICliWorkflowRun` exposes its scope.

`ICliWorkflowCommandProvider` moving to `Scoped` is breaking for anyone
resolving it from the root provider directly.

Disposal moves out of `CliWorkflowRun` and into the host, so
`TerminalCliApp` and `ArgsCliApp` both gain responsibility for it. A run
spans multiple ask/write cycles, so the trigger is "the write that
followed the command that finished the run", not every write.

Consumer artefact factories still cannot take constructor dependencies:
they are built by `Activator.CreateInstance` during
`AddArtefactFactoriesForAssembly`, not by DI. This ADR knowingly leaves
that unfixed — it is a separate defect in how those factories are
registered, not a lifetime decision.
