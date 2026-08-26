# 0013. Giving a command extra names

## What this is for

A command answers to two names derived from its type — the full
`spare-money` and the shorthand `sm`. `[CliCommandAlias]` adds any others
you want, so a better name never costs a type rename.

## How to do it

Apply it to the command type, once per name:

```csharp
[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record SpareMoneyCliCommand : CliCommand;
```

`/spare-money`, `/sm`, `/gimme` and `/give-me-cash` now all reach the same
command, built by the same factory: an alias is another key onto the
command you already have. In the playground, `/test-aliased`, `/gimme` and
`/give-me-cash` are one command, `TestAliasedCliCommand`.

Write aliases as the user types them — lowercase, dash-separated, no prefix
character. Everything after the first space is a sub-command, not part of
the name.

Aliases are names *a user types*. Chaining
([0007-chaining-commands.md](0007-chaining-commands.md)) names the next
command by type, so it never sees them; a suggestion
([0014-suggesting-what-to-run-next.md](0014-suggesting-what-to-run-next.md))
may name one, since the user types that too.

## Common mistakes

**Expecting the alias to replace the derived names.** It adds to them.
`/spare-money` and `/sm` keep working, and neither can be turned off — to
change the primary name, rename the type.

**Capitalising an alias.** Names are matched exactly, so
`[CliCommandAlias("Gimme")]` answers to `/Gimme` and not to `/gimme`.

**Aliasing to a name something else already answers to.** The alias
registers anyway and the ask resolves to whichever factory was registered
first — assembly order, not a rule you set. Nothing reports the clash, at
startup or after
([#19](https://github.com/KitCli/KitCli/issues/19)). Shorthands collide
this way too: `TestAliasedCliCommand` and `TestAggregatorCliCommand` both
stem to `ta`.

## Learn more

- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  how a type name becomes an instruction name, and how the keyed
  registration an alias adds to works.
- [../adr/0007-cli-command-alias-attribute.md](../adr/0007-cli-command-alias-attribute.md) —
  why extra names are declared on the type rather than configured
  anywhere else.
