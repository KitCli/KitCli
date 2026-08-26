# Microsoft.Extensions.DependencyInjection

KitCli builds one container, in `CliAppBuilder.Run()`, from
`Microsoft.Extensions.DependencyInjection` 10.0.2 on `net10.0`. This page
answers "can I do X with the container" — which features KitCli supports,
and where each one stops. For *why* the run scope exists, see
[ADR 0002](../adr/0002-di-scope-per-workflow-run.md).

```csharp
var serviceProvider = _services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
});
```

## Service lifetimes

| Lifetime | Supported | One instance per |
|---|---|---|
| `Singleton` | Yes | process |
| `Scoped` | Yes | workflow run |
| `Transient` | Yes | resolution |

`Scoped` means **per run, not per command**. A run that ends on a reusable
outcome keeps the same scope — and the same instances — across every
command and every ask in that run.

## Where a scoped service can be consumed

Everything that executes while the run's scope is alive should resolve
from it. Today only command handlers do —
[ADR 0010](../adr/0010-resolve-the-whole-run-from-its-scope.md) closes the
gap for the rest.

| Extension point | Today | After 0010 | Registration |
|---|---|---|---|
| Command handler | Yes | Yes | MediatR transient |
| MediatR pipeline behaviour | Yes | Yes | MediatR transient |
| `IInstructionParser` | No | Yes | singleton → transient |
| `InstructionTokenIndexer` | No | Yes | singleton → transient |
| `InstructionTokenExtractor` | No | Yes | singleton → transient |
| `IInstructionArgumentBuilder` | No | Yes | singleton → transient |
| `IInstructionValidator` | No | Yes | singleton → transient |
| `ICliWorkflowCommandProvider` | No | Yes | singleton → transient |
| `ICliCommandFactory` | No | Yes | keyed singleton → keyed transient |
| `IArtefactFactory` (built-in) | No | Yes | singleton → transient |
| `IArtefactFactory` (yours) | No | No | instance, `Activator`-built |
| `IOutcomeIoWriter` | No | Yes | singleton → transient, resolved per run |
| `ICliIo` | No | No | singleton |
| `ICliWorkflow` | No | No | singleton |
| `CliApp` | No | No | singleton |

`ICliIo`, `ICliWorkflow`, and `CliApp` stay singletons deliberately —
the host loop uses them between runs, so a run-scoped instance would be
wrong. `IOutcomeIoWriter` is excluded **today** only by timing: it runs
after the scope is disposed. ADR 0010 moves that disposal past the write
and resolves writers per run, which is why its column reads Yes.

Your own `ArtefactFactory<>`, `Aggregator<,>`, and `TableBuilder<,>`
subclasses are a separate problem. They are created by
`Activator.CreateInstance` during `AddArtefactFactoriesForAssembly` and
registered as instances, so they take no constructor dependencies at any
lifetime — adding one throws `MissingMethodException` at registration, not
a DI error. ADR 0010 does not fix this.

Handlers reach the scope today because `ISender` is a MediatR *transient*
constructed inside the run's scope, so the provider it captures is the
scope's. `ICliWorkflowCommandProvider` takes the same `IServiceProvider`
but is registered `Singleton`, so it is constructed once in the root and
captures the root provider — every factory resolving through it therefore
misses the scope, and registering it `Scoped` is what fixes that.

## The run scope's lifetime

The scope opens in `CliWorkflow.CreateNewRun()` and is disposed in
`CliWorkflowRun.UpdateStateWhenFinished()`, the moment the run reaches
`Finished`. That happens inside `ExecuteCommand`'s `finally`, so it
completes **before** `CliApp.WriteOutcomes` runs.

| When the command's last outcome is | Run state | Scope at write time |
|---|---|---|
| Non-reusable (`Anonymous`, `Final`) | `Finished` | disposed |
| `NextCliCommandOutcome` | `MovePastAsk` | alive |
| Other reusable | `ReachedReusableOutcome` | alive |

Outcomes are self-contained records, so writers are unaffected. Anything
that tried to resolve from the scope during writing would throw
`ObjectDisposedException` on the first row and succeed on the other two.

## Registration shapes

| Shape | Supported | `ValidateOnBuild` checks it |
|---|---|---|
| `AddSingleton<TService, TImpl>()` | Yes | Yes |
| `AddScoped` / `AddTransient` | Yes | Yes |
| `AddKeyedSingleton<TService, TImpl>(key)` | Yes | Yes |
| `AddSingleton(instance)` | Yes | No — no call site |
| `AddSingleton(sp => ...)` | Yes | No |
| Open generics | Yes | No |
| `IEnumerable<TService>` | Yes | Yes |

KitCli uses two of the unchecked shapes. The artefact discovery loops and
`CliAppBuilder`'s `IOptions<InstructionSettings>` register instances;
MediatR registers `ISender`, `IPublisher`, and `INotificationPublisher` as
factories and instances. No open generics are registered today — a
consumer adding an `IPipelineBehavior<,>` would introduce the first.

## Container validation

Both validations are on unconditionally — unlike ASP.NET Core, which
enables them only in Development.

| Option | Effect |
|---|---|
| `ValidateScopes` | resolving a scoped service from the root throws |
| `ValidateOnBuild` | every constructor graph is checked at startup |

**A singleton that injects `IServiceProvider` captures the root provider,
and neither validation flags it.** Injecting `IServiceProvider` is legal
at any lifetime, so the capture is invisible to `ValidateOnBuild` and only
surfaces as a scoped-from-root failure at resolution time.

## The options pattern

`CliAppBuilder.WithSettings<T>()` binds through `Configure<T>`, so all
three accessors work. `InstructionSettings` is the exception: when no
configuration source is registered it is wrapped in an `OptionsWrapper<T>`
and registered as an instance, so only `IOptions<T>` resolves and the
value is fixed for the process.

| Accessor | Lifetime | Available for |
|---|---|---|
| `IOptions<T>` | Singleton | any bound settings |
| `IOptionsSnapshot<T>` | Scoped | `WithSettings<T>` only |
| `IOptionsMonitor<T>` | Singleton | `WithSettings<T>` only |

`IOptionsSnapshot<T>` is scoped, so only a command handler can take it.

## Gaps

No issue tracks these yet. They are the scope of the *Scope Dependency
Injection Per Workflow Run* milestone.

- No extension point except a command handler can reach the run scope.
- The run scope is disposed before outcomes are written.
- `IOutcomeIoWriter.Write(Outcome)` has no context parameter, so there is
  no equivalent of `HttpContext.RequestServices`.
- Consumer artefact factories cannot take constructor dependencies.
- `WithJsonSettings` passes `reloadOnChange: true`, which
  `InstructionSettings` cannot honour.

## See also

- [ADR 0002 — DI scope per workflow run](../adr/0002-di-scope-per-workflow-run.md)
- [ADR 0010 — resolve the whole run from its scope](../adr/0010-resolve-the-whole-run-from-its-scope.md)
- [Workflow run state machine](../concepts/0010-workflow-run-state-machine.md)
- [CliApp host loop](../concepts/0002-cli-app-host.md)
