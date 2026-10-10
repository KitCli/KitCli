# 0011. How should a fallback get added when a sibling sub-command already has a factory?

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-10

## Verdict

**New complexity.** The one-line fix that looks obvious — always register the
automatic `BasicCliCommandFactory<T>` too, even when a dedicated factory
already exists — is actively wrong. It was written and run: it does fix the
reported crash, but it also makes a *different* ask build the wrong command,
silently, where today it loudly fails instead.

## Recommendation

Two candidate fixes, with a real tradeoff. Pick one before turning
[#289](https://github.com/KitCli/KitCli/issues/289) into delivery work.

1. **Naming-convention stopgap.** Recognise a factory as already covering a
   sibling command when the factory's class name is `{CommandTypeName}Factory`
   (every factory in the codebase already happens to be named this way).
   Anything not matched this way gets an automatic fallback, keyed under the
   *group's* name rather than its own. Small — one method — but it's a new
   naming contract nothing enforces today; renaming a factory silently drops
   its coverage.
2. **Wait on the command-descriptor epic.** [#188](https://github.com/KitCli/KitCli/issues/188),
   via [#190](https://github.com/KitCli/KitCli/issues/190) and
   [#192](https://github.com/KitCli/KitCli/issues/192), has a factory declare
   `SubCommandIs("there")` as data, read at registration before any
   instruction exists. Once that lands, registration can read what a factory
   actually covers instead of guessing from its name. Correct, but depends on
   three issues that haven't shipped, and none were scoped with this gap in
   mind.

## What was established

1. **One instruction name serves a whole family of sibling commands.** `/hello`,
   `/hello there`, and `/hello mate` all parse to the same `instruction.Name`
   ("hello") — only `SubInstructionName` differs.
2. **That's why sibling factories share `<T>`.** The documented sub-command
   pattern ([command-registration.md](../concepts/command-registration.md),
   [#283](https://github.com/KitCli/KitCli/issues/283)) requires every factory
   in a family to declare the *primary* command's type as `<T>`, whatever
   sibling its `Create()` actually returns — that's what puts them under one
   registration key, with `CanCreateWhen()` the only thing telling them apart.
3. **So `<T>` can't say what's already covered.** `AddCommandFactories`'s match
   (`factory.BaseType.FirstGenericArgumentIs(commandType)`) can only answer
   "does any factory exist for this family," never "for this one sibling."
4. **Confirmed the reported failure.** One dedicated factory covering one
   sub-command leaves every other ask in the family throwing, bare command
   included — not building the wrong thing, just failing outright.
5. **Confirmed the obvious fix trades that loud failure for a silent one.**
   Always registering the fallback alongside a dedicated factory fixes the
   bare case, but an uncovered sub-command stops throwing too — it silently
   builds the *primary* command instead of its own.

## Evidence

Registered one family — `HelloCliCommand`, `HelloThereCliCommand`,
`HelloMateCliCommand` — with a factory only for `there`:

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Resolved `/hello`, `/hello there`, `/hello mate` through
`CliWorkflowCommandProvider.GetCommand` against `bbea37e` (main), then again
after removing the `if (matchingFactories.Count > 0) { ...; continue; }`
guard in `CommandServiceCollectionExtensions.AddCommandFactories` so the
fallback always registers too:

| Ask | Today | With the fallback always registered |
|---|---|---|
| `/hello` | throws `NoCommandGeneratorException` | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | throws the same exception | builds `HelloCliCommand` — **wrong**, no exception |

## Open questions

- Does this gap get its own fix, or fold entirely into [#188](https://github.com/KitCli/KitCli/issues/188)'s
  descriptor work?
- If the naming-convention stopgap ships first, does it get retired once the
  descriptor epic lands, or does it stay as a permanent, cheaper path?

## Out of scope

- Redesigning the shared-`<T>` sub-command pattern itself — settled,
  already used elsewhere, and not what's in question here.
- Multi-word sub-command paths — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
