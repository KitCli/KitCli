# Writing a basic command

## What this is for

Every piece of behavior in a KitCli app is a command: a marker type
the user's typed ask resolves to, plus a handler that does the work.
This is the minimum you need to add one.

## How to do it

Three pieces, all in the assembly your app registers:

```csharp
// 1. The command — just a marker type.
public record GreetCliCommand(string Name) : CliCommand;

// 2. The handler — does the actual work.
public class GreetCliCommandHandler : CliCommandHandler<GreetCliCommand>
{
    public override Task<Outcome[]> HandleCommand(GreetCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByFinallySaying($"Hello, {command.Name}!")
            .EndAsync();
}
```

That's it — no factory needed here. A command's invocation name is
derived automatically from its type name: `GreetCliCommand` → `/greet`
(and the shorthand `/g`, the first letter of each capitalized word).
Register every command in the assembly once, from your
`ICliAppRegistry`:

```csharp
public class MyAppRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
        => services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
}
```

### When you actually need a factory

`GreetCliCommand` above has a parameterless-constructor-free record
whose args come straight from the constructor — but a command factory
is what builds a command from the current ask's typed arguments,
prior artefacts, or arbitrary decision logic. Write one only when the
command needs one of those to be constructed:

```csharp
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

If you don't write one, and the command type has a public
parameterless constructor, KitCli builds one for you automatically
(`BasicCliCommandFactory<T>`) — that's what made the first example
above work with zero factory code.

## Common mistakes

**Writing a factory for every command out of habit.** If a command has
a parameterless constructor and needs no arguments, artefacts, or
conditional creation logic, a factory is pure boilerplate — skip it.

**Forgetting `CanCreateWhen()` decides whether the command is offered
at all, not just how it's built.** Returning `false` doesn't fail
loudly — the instruction resolves to "no matching command" instead
(see [reading-command-arguments.md](reading-command-arguments.md) for
reading the arguments `CanCreateWhen` and `Create` would use to decide).

**Expecting the handler to see the raw typed ask.** By the time
`HandleCommand` runs, argument parsing is already done — the command
instance you constructed (in a factory, or via its constructor) is all
the handler ever sees.

## Learn more

- [reading-command-arguments.md](reading-command-arguments.md) — using
  arguments from the current ask inside a factory.
- [creating-a-registry.md](creating-a-registry.md) — wiring
  `AddCommandsFromAssembly` up for a real app, including
  settings-driven registries.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  how a command's name/shorthand is derived, and exactly how
  `AddCommandsFromAssembly` decides between your factory and the
  automatic one.
