# 0017. Giving a command type more than one factory

## What this is for

One command type can be built differently depending on which sub-command
word it was asked with — a `user` command with `add`, `edit`, and `delete`
variants, say. Give each variant its own `CliCommandFactory<T>` instead of
branching inside a single `Create()`.

## How to do it

Write one `CliCommandFactory<T>` per variant, each gated by its own
`CanCreateWhen()`:

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

`AddCommandsFromAssembly` registers both factories under `UserCliCommand`'s
name. `/user add` and `/user edit` each resolve, first-match-wins across
whichever factories return `true`. `TestMultipleFactoriesCliCommand` in
`KitCli.Playground.Scenarios` is a runnable copy: try
`/test-multiple-factories a` and `/test-multiple-factories b`.

## Common mistakes

**Expecting a fourth, uncovered sub-command to give a helpful error.** No
factory returning `true` means the ask fails to resolve, exactly like a
typo — nothing distinguishes "close but no match" from "not a command at
all".

**Two factories claiming the same sub-command.** Whichever one is
discovered first wins, silently. Keep every `CanCreateWhen()` condition
mutually exclusive.

**Reaching for this when one factory would do.** Split only once what you
populate, or how, differs enough to make a single branching `Create()`
hard to follow.

## Learn more

- [0006-gating-a-command-with-cancreatewhen.md](0006-gating-a-command-with-cancreatewhen.md) —
  how `CanCreateWhen` decides whether one factory's command is offered at
  all.
- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  how registration keys factories by name and resolves them first-match-wins.
