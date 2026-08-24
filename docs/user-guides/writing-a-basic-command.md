# Writing a basic command

## What this is for

Every piece of behavior in a KitCli app is a command: a marker type the
user's ask resolves to, plus a handler that does the work. This is the
minimum to add one.

## How to do it

Two pieces, in the assembly your app registers:

```csharp
// 1. The command — just a marker type.
public record GreetCliCommand : CliCommand;

// 2. The handler — does the work.
public class GreetCliCommandHandler : CliCommandHandler<GreetCliCommand>
{
    public override Task<Outcome[]> HandleCommand(GreetCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByFinallySaying("Hello!")
            .EndAsync();
}
```

No factory needed. A command's invocation name comes from its type name:
`GreetCliCommand` → `/greet`, plus the shorthand `/g`, the first letter of
each capitalized word.

**Name the type `...CliCommand`, not `...Command`.** The derivation strips
the word `CliCommand`. A type called `GreetCommand` keeps its suffix and
answers to `/greet-command`.

Commands in the same assembly as your `Program.cs` register themselves.
For commands anywhere else, register that assembly once from your
`ICliAppRegistry`:

```csharp
public class MyAppRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
        => services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
}
```

### When you need a factory

A factory builds a command from the current ask's typed arguments, prior
artefacts, or a runtime decision. Write one when your command needs any of
those, which includes every command with constructor parameters — a
`GreetCliCommand` taking the name to greet:

```csharp
public record GreetCliCommand(string Name) : CliCommand;

public class GreetCliCommandFactory : CliCommandFactory<GreetCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var name = GetRequiredArgument<string>("name").Value;
        return new GreetCliCommand(name);
    }
}
```

Three base classes exist, so you write only the half you care about:

| Base class | Write | Use when |
|---|---|---|
| `CliCommandFactory<T>` | both methods | you need custom building *and* gating |
| `BasicCreationCliCommandFactory<T>` | `Create()` only | the command always applies, but needs building |
| `BasicDecisionCliCommandFactory<T>` | `CanCreateWhen()` only | the command is `new T()`, but applies only sometimes |

Skip the factory when the command type has a public parameterless
constructor and needs no gating. KitCli registers
`BasicCliCommandFactory<T>` for it, which is what makes the first example
above work with no factory code.

## Common mistakes

**Writing a factory for every command out of habit.** Given a
parameterless constructor and no need for arguments, artefacts, or
conditional creation, a factory is pure boilerplate. Skip it.

**Giving a command constructor parameters but no factory.** The automatic
`BasicCliCommandFactory<T>` covers only types with a public parameterless
constructor. A positional record like `GreetCliCommand(string Name)` with
no factory gets no factory at all, and startup says nothing. The command
is unreachable when someone types its name.

**Forgetting that `CanCreateWhen()` decides whether the command is offered
at all, not only how it is built.** Returning `false` fails quietly: the
instruction resolves to "no matching command." See
[reading-command-arguments.md](reading-command-arguments.md) for the
arguments `CanCreateWhen` and `Create` read to decide.

**Expecting the handler to see the raw ask.** Argument parsing finishes
before `HandleCommand` runs. The command instance you constructed, in a
factory or through its constructor, is all the handler sees.

## Learn more

- [reading-command-arguments.md](reading-command-arguments.md) — using
  arguments from the current ask inside a factory.
- [creating-a-registry.md](creating-a-registry.md) — wiring
  `AddCommandsFromAssembly` up for a real app, settings-driven registries
  included.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  how a command's name and shorthand are derived, and how
  `AddCommandsFromAssembly` chooses between your factory and the automatic
  one.
