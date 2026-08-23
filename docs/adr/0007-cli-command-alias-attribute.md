# 0007. Add extra instruction names via a `[CliCommandAlias]` attribute

Status: Accepted
Date: 2026-08-23

## Context

A `CliCommand`'s instruction name is derived mechanically from its type
name — strip the `Command` suffix, dash-split, lowercase — plus a
shorthand derived from its uppercase letters (see
[command-registration.md](../concepts/command-registration.md)). Renaming
the type is the only way to change either. That leaves no way to give a
command a memorable or user-facing name that doesn't fit the
PascalCase-to-dashed convention, or to give it more than one name, without
also changing what the type is called everywhere else in the codebase.

ADR 0003 considered and rejected "keyed DI registration driven by an
explicit name argument at registration time" in favor of pure type-name
derivation, to avoid a return to hand-maintained registration lists. That
decision is about how names are *supplied*, not whether a command can have
more than one — declaring extra names on the command type itself keeps
naming type-driven and reflection-discovered, so it doesn't reopen that
trade-off.

## Decision

Add `CliCommandAliasAttribute` (`KitCli.Commands.Abstractions/CliCommandAliasAttribute.cs`),
an `AttributeUsage(AttributeTargets.Class, AllowMultiple = true)` attribute
taking a single `Name`. `AddCommandFactory`
(`CommandServiceCollectionExtensions.cs`) reads every `[CliCommandAlias]` on a
command type via `GetCustomAttributes<CliCommandAliasAttribute>()` and registers
one additional `AddKeyedSingleton<ICliCommandFactory>` per alias, using the
same factory type already resolved for that command's full/shorthand
names.

```csharp
[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record SpareMoneyCommand : CliCommand;
```

## Alternatives considered

- **A `name:` constructor argument on `CliCommand` itself** — rejected
  because it would let a command override its *primary* name, not just add
  to it, which would break the guarantee that the full name is always
  predictable from the type name (see the "Questions & answers" section of
  `command-registration.md`).
- **A single `Aliases` array on one attribute instance** — rejected in
  favor of a repeatable single-name attribute; `[CliCommandAlias("a")]
  [CliCommandAlias("b")]` reads better at the declaration site than
  `[CliCommandAlias("a", "b")]` and needs no array-syntax explanation.
- **Free-standing alias registration (e.g. a static map from command type
  to names, configured separately from the type)** — rejected for the same
  reason ADR 0003 rejected hand-maintained registration: it reintroduces a
  place to forget to update when a command is added, renamed, or removed.

## Consequences

Command authors can now give a command any number of memorable names
without touching the type name, at the cost of one more thing
(`[CliCommandAlias]`) to know about when reading a command's full set of
instruction names — they're no longer all visible from the type name
alone. Alias-to-name collisions across different command types fail at
the same point full/shorthand-name collisions already do: silently, at
first resolution, not at startup (see the existing "Registration-time
failure for type-level ambiguity; runtime failure for name-level
ambiguity" trade-off in `command-registration.md`).
