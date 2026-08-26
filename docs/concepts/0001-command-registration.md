# 0001. Command registration

You never tell KitCli that a command exists. `AddCommandsFromAssembly`
scans an assembly, finds every `CliCommand`, and wires each one to a name,
a factory, and a MediatR handler. So the only question worth answering is
the one you cannot see from your own code: **what name did my command end
up with?**

## The name comes from the type name

`GetInstructionName()` removes `CliCommand` from the type name, inserts `-`
before each uppercase letter except the first, and lowercases it. The
shorthand keeps only the uppercase letters:

```
SpareMoneyCliCommand  →  SpareMoney  →  spare-money   and   sm
```

**The string removed is `CliCommand`, not a trailing `Command`, and it goes
wherever it appears.** So `SpareMoneyCommand` keeps its suffix and becomes
`spare-money-command`. Name command types `...CliCommand`.

`[CliCommandAlias("gimme")]` adds further names, repeatable, author-chosen.

## Which factory runs

A factory is what turns the name a user typed into a command object.
Registration pairs each command type with the `CliCommandFactory<>` whose
generic argument is that type:

| Factories found for the type | Result |
|---|---|
| exactly one | registered under the full name, shorthand, and aliases |
| more than one | throws at startup, "Multiple factories found for command type" |
| none, and the command has a parameterless constructor | `BasicCliCommandFactory<T>` registered for you |
| none, and it has not | no factory, and nothing says so until a user types the name |

When an ask arrives, `CliWorkflowCommandProvider.GetCommand` fetches every
factory keyed under that name, attaches the instruction and the run's
artefacts, and takes the **first** whose `CanCreateWhen()` returns `true` —
the same first-match-wins rule used for argument builders and outcome
writers ([ADR 0004](../adr/0004-first-match-wins-resolution.md)).

## Gaps

Two command types stemming to one instruction name fail only when a user's
ask resolves there, not at startup. Tracked as
[#19](https://github.com/KitCli/KitCli/issues/19).

## See also

[0005-instruction-parsing-pipeline.md](0005-instruction-parsing-pipeline.md) ·
[0008-artefacts.md](0008-artefacts.md) ·
[../user-guides/0013-giving-a-command-extra-names.md](../user-guides/0013-giving-a-command-extra-names.md) ·
[../adr/0007-cli-command-alias-attribute.md](../adr/0007-cli-command-alias-attribute.md)
