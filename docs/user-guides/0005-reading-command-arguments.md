# 0005. Reading command arguments

## What this is for

An ask can carry `--name value` pairs: `/greet --name Alex`. A factory's
argument helpers hand you those values already typed, so you never parse a
raw string.

## How to do it

Inside a `CliCommandFactory<T>`, read arguments by type and name:

```csharp
public class GreetCliCommandFactory : CliCommandFactory<GreetCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var name = GetRequiredArgument<string>("name").Value;
        var shout = GetArgument<bool>("shout")?.Value ?? false;

        return new GreetCliCommand(name, shout);
    }
}
```

`/greet --name Alex --shout` resolves `name` to `"Alex"` and `shout` to
`true` — a bool argument written with no value resolves to `true`,
following the usual CLI idiom. `/greet --name Alex` leaves `shout` unset,
so the `?? false` default applies.

Three helpers exist, each generic over the argument's target type:

```csharp
protected InstructionArgument<T>? GetArgument<T>(string? argumentName);
protected InstructionArgument<T> GetRequiredArgument<T>(string? argumentName);  // throws if missing
protected bool AnyArgument<T>(string? argumentName);
```

Passing `null` for `argumentName` matches the last argument of that type,
whatever its name. Reserve that for a command taking exactly one argument
of the type; otherwise name arguments explicitly.

### What decides an argument's type

Not the command. **A value's type comes from what the value looks like.**
Each raw string is offered to these in order, and the first to claim it
wins:

| Type | Claims a value that... | Example |
|---|---|---|
| `DirectoryInfo` | is a rooted path, or starts with `.` | `--out ./reports` |
| `Guid` | parses as a GUID | `--id 6f9e...` |
| `string` | has at least one letter, and isn't `true`/`false` | `--name Alex` |
| `int` | parses as a whole number | `--count 5` |
| `decimal` | parses as a decimal | `--limit 12.50` |
| `DateOnly` | parses as a date | `--due 2026-03-01` |
| `bool` | anything left, including no value at all | `--shout` |

The "has a letter" rule on `string` keeps `--count 5` an `int`. Conversely
`--name 42` is an `int`, not a `string`. Consuming code cannot add a type
beyond this list today; see
[../concepts/0005-instruction-parsing-pipeline.md](../concepts/0005-instruction-parsing-pipeline.md)
for why.

## Common mistakes

**Calling `GetRequiredArgument<T>` for something optional.** It throws when
the argument is missing. Use `GetArgument<T>` with a fallback for anything
a user might reasonably omit.

**Asking for the wrong type.** `GetArgument<T>` finds only an argument the
rules above typed as `T`. `GetArgument<int>("page")` returns `null` for all
three of `--page` (no value, so it became a `bool`), `--page abc` (has
letters, so it became a `string`), and no `--page` at all. Nothing
distinguishes "present but the wrong shape" from "absent".

**Reaching for `GetArgument` inside a handler.** These helpers live on
`CliCommandFactory<T>` alone. By the time a handler runs, the command
instance holds whatever data it needs.

## Learn more

- [0001-writing-a-basic-command.md](0001-writing-a-basic-command.md) — where a
  factory fits, and when you need one.
- [../concepts/0005-instruction-parsing-pipeline.md](../concepts/0005-instruction-parsing-pipeline.md) —
  how a raw ask becomes typed arguments.
- [../concepts/0008-artefacts.md](../concepts/0008-artefacts.md) —
  `GetArtefact` and `GetRequiredArtefact`, the equivalents for data from
  *prior* commands in the same run.
