# 0017. Creating sub-commands

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
[0009-how-many-sub-commands-can-one-instruction-address.md](../investigations/0009-how-many-sub-commands-can-one-instruction-address.md).

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

- [0006-gating-a-command-with-cancreatewhen.md](0006-gating-a-command-with-cancreatewhen.md) —
  how `CanCreateWhen` decides whether one factory's command is offered at
  all.
- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  how registration keys factories by name and resolves them first-match-wins.
