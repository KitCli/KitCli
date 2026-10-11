# 0011. Handling fallback command factories when sub commands are in use

- **Status:** In Review
- **Spike:** not yet filed — ran inline, while triaging [#289](https://github.com/KitCli/KitCli/issues/289)
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-11

## Verdict

**New complexity.**

A KitCli command is a word someone types after a slash, like `/hello`. A
sub-command is an extra word right after it, like `/hello there`, asking
for one specific action instead of the plain one.

Each command needs a factory: the code that builds the object
representing that exact ask, so KitCli knows which handler runs next.
Most commands don't need a hand-written factory — if building the object
takes nothing beyond `new()`, KitCli writes that factory for you. Call
that the **automatic fallback**.

Writing one hand-written factory, for one sub-command, currently turns
the automatic fallback off for every other command sharing the same
name — the plain command included, and any other sub-command that still
needed nothing but `new()`.

Leaving the automatic fallback switched on even when a hand-written
factory exists was tried as the fix. It does stop the plain command from
crashing. But it also makes KitCli silently build the wrong command for a
sub-command nobody wrote a factory for, instead of throwing the exception
it throws today. A wrong answer with no error is worse than a crash.

## Recommendation

Three candidate fixes, each with a real cost. Pick one before turning
[#289](https://github.com/KitCli/KitCli/issues/289) into work to build —
the third needs its own estimate, not #289's current "small fix" label.

1. **Guess coverage from the factory's class name.** Every hand-written
   factory in this codebase is already named after the exact command it
   builds — `HelloThereCliCommandFactory` builds `HelloThereCliCommand`.
   Treat a name match as proof that command is covered, and give the
   automatic fallback to everything else sharing its command name. Cheap,
   but fragile: renaming a factory would silently switch its coverage off.
2. **Wait for commands to declare what they need.** Separate, larger work
   is already planned ([#188](https://github.com/KitCli/KitCli/issues/188)).
   It would let a factory state, as data KitCli can read before running
   anything, exactly which sub-command it's for — no guessing. The right
   long-term answer, but it depends on unfinished work not planned with
   this problem in mind.
3. **Give `CliCommandFactory<T>` a second type parameter, for what it
   builds** — worked through below.

### Option 3, worked through

Today, every factory for commands sharing one name has to declare the
same type parameter: the plain command's own type. `HelloThereCliCommandFactory`
is declared as `CliCommandFactory<HelloCliCommand>`, even though its
`Create()` method returns a `HelloThereCliCommand`. Declaring
`HelloCliCommand` is what gets it looked up under "hello"; `CanCreateWhen()`
is what decides, once someone actually types a command, whether this
specific factory applies.

Because every factory sharing "hello" declares that same type parameter,
KitCli has no way to ask "does a factory already exist for
`HelloMateCliCommand` specifically?" The type parameter only ever answers
"does a factory exist for the `hello` group."

The fix: give `CliCommandFactory<T>` a second type parameter, for what
the factory actually builds:

```csharp
public class HelloThereCliCommandFactory
    : CliCommandFactory<HelloCliCommand, HelloThereCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override HelloThereCliCommand Create() => new HelloThereCliCommand();
}
```

`HelloCliCommand` still groups it under "hello." `HelloThereCliCommand` is
new: the exact type `Create()` returns, declared on the class itself. With
this, KitCli could check, by reflection, whether any factory's second type
parameter equals `HelloMateCliCommand`. A match would mean a factory
already covers it; no match would mean it doesn't — a real check, not a
guess at a class name.

That only answers whether `HelloMateCliCommand` is covered. If it isn't,
KitCli still has to build it an automatic fallback, and the second type
parameter doesn't answer two things that fallback needs: which group to
register it under ("hello," not something derived from
`HelloMateCliCommand`'s own name), and which sub-command word should
trigger it ("mate"). Both still have to come from guessing at
`HelloMateCliCommand`'s name — the same guess option 1 makes.

It's also a breaking change: every `CliCommandFactory<T>` written today
takes one type parameter, not two.

## What was established

1. **Every factory sharing a command name shares that command's type
   parameter.** A hand-written factory for `/hello there` or `/hello mate`
   has to declare `CliCommandFactory<HelloCliCommand>` — never
   `CliCommandFactory<HelloThereCliCommand>` or
   `CliCommandFactory<HelloMateCliCommand>` — so KitCli looks it up under
   "hello," the same name as the plain command.
2. **That's why nothing today distinguishes a covered sub-command from an
   uncovered one.** Every factory sharing a command name declares the
   identical type parameter. Which sub-command a factory actually covers
   lives only inside its `CanCreateWhen()` method, unreadable without
   running it.
3. **Confirmed the reported crash.** With `HelloThereCliCommandFactory` as
   the only hand-written factory, both `/hello` and `/hello mate` threw
   `NoCommandGeneratorException`; `/hello there` built `HelloThereCliCommand`
   correctly.

## Evidence

Wrote one small family by hand: a plain `HelloCliCommand`, and two
sub-commands, `HelloThereCliCommand` and `HelloMateCliCommand`. Wrote a
factory for `there` only — the other two need nothing beyond `new()`:

```csharp
public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("there");
    public override CliCommand Create() => new HelloThereCliCommand();
}
```

Typed `/hello`, `/hello there`, and `/hello mate` through KitCli's actual
command-resolution code on `main` (`bbea37e`), then again after removing
the guard that stops the automatic fallback registering whenever a
dedicated factory already exists:

| Typed | Today | With the automatic fallback always registered |
|---|---|---|
| `/hello` | throws `NoCommandGeneratorException: Did not find command factory for hello` | builds `HelloCliCommand` — fixed |
| `/hello there` | builds `HelloThereCliCommand` | unchanged |
| `/hello mate` | throws the same exception as `/hello` | builds `HelloCliCommand` — **wrong command, no exception** |

Option 3 is a design sketch, not yet written or run — nothing has
confirmed it compiles or behaves as described.

## Open questions

- Does this get its own fix, fold into [#188](https://github.com/KitCli/KitCli/issues/188)'s
  larger work, or does option 1 ship first and retire once that work lands?

## Out of scope

- Sub-commands more than one word long — covered by investigation
  [0009](0009-how-many-sub-commands-can-one-instruction-address.md).
