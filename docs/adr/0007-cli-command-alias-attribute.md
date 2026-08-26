# 0007. Add extra instruction names via a `[CliCommandAlias]` attribute

Status: Accepted
Date: 2026-08-23

## Context

A command's instruction name is derived mechanically from its type name —
remove `CliCommand`, dash-split, lowercase — plus a shorthand from its
uppercase letters (see
[0001-command-registration.md](../concepts/0001-command-registration.md)).
Renaming the type was the only way to change either. So a command could not
have a memorable user-facing name that does not fit the PascalCase
convention, nor more than one name, without renaming the type everywhere.

[ADR 0003](0003-reflection-based-automatic-registration.md) rejected
supplying names at registration time, to avoid returning to hand-maintained
lists. That is about how names are *supplied*, not how many a command may
have — declaring extras on the type keeps naming type-driven, so it does
not reopen that trade-off.

## Decision

Add `CliCommandAliasAttribute`, an
`AttributeUsage(AttributeTargets.Class, AllowMultiple = true)` attribute
taking a single `Name`. `AddCommandFactory` reads every one off a command
type and registers one additional keyed `ICliCommandFactory` per alias,
using the same factory type already resolved for that command.

```csharp
[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record SpareMoneyCliCommand : CliCommand;
```

## Alternatives considered

- **A `name:` constructor argument on `CliCommand`** — would let a command
  override its *primary* name, breaking the guarantee that the full name is
  predictable from the type name.
- **One attribute taking an `Aliases` array** — `[CliCommandAlias("a")]
  [CliCommandAlias("b")]` reads better at the declaration site and needs no
  array syntax explained.
- **A free-standing map from command type to names** — reintroduces the
  place to forget to update, for the reason ADR 0003 gave.

## Consequences

A command can have any number of memorable names without a type rename, at
the cost of one more thing to know when reading a command's full set of
names — they are no longer all visible from the type name alone.
Alias collisions across command types fail where full-name and shorthand
collisions already do: silently, at first resolution rather than at
startup. Tracked as [#19](https://github.com/KitCli/KitCli/issues/19).
