# 0001. Command registration

`AddCommandsFromAssembly` scans an assembly and wires every `CliCommand`
to a name, a factory, and a MediatR handler. You register nothing by hand,
so the only thing worth knowing is what name your command ends up with.

## Naming

`GetInstructionName()` removes `CliCommand` from the type name, inserts
`-` before each uppercase letter except the first, and lowercases it. The
shorthand keeps only the uppercase letters:

```
SpareMoneyCliCommand  →  SpareMoney  →  spare-money   and   sm
```

**The string removed is `CliCommand`, not a trailing `Command`, and it is
removed everywhere it appears.** So `SpareMoneyCommand` keeps its suffix
and becomes `spare-money-command`. Name command types `...CliCommand`.

`[CliCommandAlias("gimme")]` adds further names, repeatable, author-chosen.

## Which factory runs

Registration matches each command type to the `CliCommandFactory<>` whose
generic argument is that type:

| Match | Result |
|---|---|
| exactly one | registered under the full name, shorthand, and aliases |
| more than one | throws at startup, "Multiple factories found for command type" |
| none, parameterless constructor | `BasicCliCommandFactory<T>` registered for you |
| none, no parameterless constructor | no factory, and nothing says so until a user types the name |

At runtime `CliWorkflowCommandProvider.GetCommand` fetches every factory
keyed under the instruction name, attaches the instruction and artefacts,
and takes the **first** whose `CanCreateWhen()` returns `true` — the same
first-match-wins rule used for argument builders and outcome writers.

## Gaps

Two command types stemming to one instruction name fail only when a user's
ask resolves there, not at startup. Tracked as
[#19](https://github.com/KitCli/KitCli/issues/19).

## See also

[0005-instruction-parsing-pipeline.md](0005-instruction-parsing-pipeline.md) ·
[../user-guides/0013-giving-a-command-extra-names.md](../user-guides/0013-giving-a-command-extra-names.md) ·
[0008-artefacts.md](0008-artefacts.md) ·
[0007-cli-command-alias-attribute.md](../adr/0007-cli-command-alias-attribute.md)
