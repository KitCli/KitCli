# Creating sub-commands

## What this is for

One instruction name can behave differently depending on the word that
follows it — `/user add` and `/user edit` both start a `user` command, but
each should do something different. Give each word its own
`CliCommandFactory<T>`, gated by `SubCommandIs(...)`, instead of branching
inside a single `Create()`.

## How to do it

Write one `CliCommandFactory<T>` per sub-command word:

```csharp
public record UserCliCommand(string Action) : CliCommand;

public class AddUserCliCommandFactory : CliCommandFactory<UserCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("add");
    public override CliCommand Create() => new UserCliCommand("add");
}

public class EditUserCliCommandFactory : CliCommandFactory<UserCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("edit");
    public override CliCommand Create() => new UserCliCommand("edit");
}
```

`AddCommandsFromAssembly` registers every factory it finds for
`UserCliCommand` under the one name, `user`. Typing `/user add` or `/user
edit` tries each factory in turn and runs the first whose `CanCreateWhen()`
says yes. `TestMultipleFactoriesCliCommand` in `KitCli.Playground.Scenarios`
is a runnable copy of this.

A sub-command is one word today. `/user edit team lead` arrives as a single
glued string, `"edit team lead"`, not three words to match against — see
[how-many-sub-commands-can-one-instruction-address.md](../investigations/how-many-sub-commands-can-one-instruction-address.md).

### Building a different command for a sub-command word

A factory's type argument sets the name the user types, but `Create()` can
return any command. To let users type `/source choose` and run the asking
command from
[Asking for a missing value and carrying on](asking-for-a-missing-value-and-carrying-on.md),
add a command that only supplies the word `source`:

```csharp
public record SourceCliCommand : CliCommand;

public class SourceChooseCliCommandFactory : CliCommandFactory<SourceCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("choose");
    public override CliCommand Create() => new ChooseSourceCliCommand(ReturnTo: null);
}
```

`SourceCliCommand` never runs, so it needs no handler. In an interactive
app, the user's choice is remembered, and their next `/export` reads it.

## Common mistakes

**Typing an unlisted word and expecting an error.** `/user delete`, with no
matching factory, fails to resolve exactly like a typo.

**Two factories matching the same word.** Whichever one KitCli finds first
wins, silently. Keep every `CanCreateWhen()` mutually exclusive.

**Splitting into factories with nothing to split.** If every sub-command
builds the same fields, one factory branching on `SubCommandIs(...)` inside
`Create()` is simpler than several. Split only when a sub-command needs its
own constructor arguments.

## Learn more

- [Gating a command with CanCreateWhen](gating-a-command-with-cancreatewhen.md) —
  how `CanCreateWhen` decides whether one factory's command is offered at
  all.
- [../concepts/command-registration.md](../concepts/command-registration.md) —
  how registration keys factories by name and resolves them first-match-wins.
