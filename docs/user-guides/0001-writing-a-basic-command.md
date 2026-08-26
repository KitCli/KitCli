# 0001. Writing a basic command

## What this is for

Every piece of behaviour in a KitCli app is a command. Write one and the
user can type its name. This is the smallest version — two types, no
registration code.

## How to do it

Put both in the assembly your app registers:

```csharp
// 1. The command — a marker type. It holds data, never behaviour.
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

The name comes from the type name: `GreetCliCommand` answers to `/greet`,
plus the shorthand `/g` — the first letter of each capitalised word.

**Name the type `...CliCommand`, not `...Command`.** The derivation strips
the word `CliCommand`. A type called `GreetCommand` keeps its suffix and
answers to `/greet-command`.

Commands beside your `Program.cs` register themselves. For commands
anywhere else, name that assembly once from your `ICliAppRegistry`:

```csharp
public class MyAppRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
        => services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
}
```

### When you need a factory

A factory decides whether the command applies right now, and builds it.
Write one when your command needs the ask's arguments, an earlier command's
data, or a runtime decision — which includes every command with constructor
parameters:

```csharp
public record GreetCliCommand(string Name) : CliCommand;

public class GreetCliCommandFactory : CliCommandFactory<GreetCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
        => new GreetCliCommand(GetRequiredArgument<string>("name").Value);
}
```

Three base classes exist, so you write only the half you care about:

| Base class | Write | Use when |
|---|---|---|
| `CliCommandFactory<T>` | both methods | you need custom building *and* gating |
| `BasicCreationCliCommandFactory<T>` | `Create()` only | the command always applies, but needs building |
| `BasicDecisionCliCommandFactory<T>` | `CanCreateWhen()` only | the command is `new T()`, but applies only sometimes |

Skip the factory when the command has a public parameterless constructor
and needs no gating. KitCli registers `BasicCliCommandFactory<T>` for it,
which is what makes the first example work with no factory code.

## Common mistakes

**Writing a factory for every command out of habit.** With a parameterless
constructor and no need for arguments, artefacts, or a decision, a factory
is pure boilerplate.

**Giving a command constructor parameters but no factory.** The automatic
factory covers only types KitCli can build with `new`. A record like
`GreetCliCommand(string Name)` with no factory gets none at all, startup
says nothing, and the command is unreachable when someone types its name.

**Expecting the handler to see the raw ask.** Parsing finishes before
`HandleCommand` runs. The command instance is all the handler gets.

## Learn more

- [0005-reading-command-arguments.md](0005-reading-command-arguments.md) — the
  arguments a factory reads.
- [0006-gating-a-command-with-cancreatewhen.md](0006-gating-a-command-with-cancreatewhen.md) —
  what `CanCreateWhen` is for, and how a `false` fails.
- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  how the name is derived and how the factory is chosen.
