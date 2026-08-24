# Reading command arguments

## What this is for

A user's ask can carry `--name value` pairs: `/greet --name Alex`. A
command factory's argument helpers hand you those values already typed,
so you never parse a raw string.

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
`true`; a bool argument written with no value — a bare flag — resolves to
`true`, following the usual CLI idiom. `/greet --name Alex` leaves `shout`
unset, so the `?? false` default applies.

Three helpers exist, each generic over the argument's target type:

```csharp
protected InstructionArgument<T>? GetArgument<T>(string? argumentName);
protected InstructionArgument<T> GetRequiredArgument<T>(string? argumentName);  // throws if missing
protected bool AnyArgument<T>(string? argumentName);
```

Passing `null` for `argumentName` matches the last argument of that type,
whatever its name. Reserve that for a command taking exactly one argument
of the type; otherwise name arguments explicitly.

### Supported argument types

A value's type comes from what it looks like, never from what the command
declares. Each raw string is offered to these in order, and the first to
claim it wins:

| Type | Claims a value that... | Example |
|---|---|---|
| `DirectoryInfo` | is a rooted path, or starts with `.` | `--out ./reports` |
| `Guid` | parses as a GUID | `--id 6f9e...` |
| `string` | has at least one letter, and isn't `true`/`false` | `--name Alex` |
| `int` | parses as a whole number | `--count 5` |
| `decimal` | parses as a decimal | `--limit 12.50` |
| `DateOnly` | parses as a date | `--due 2026-03-01` |
| `bool` | anything left, including no value at all | `--shout` |

The "has a letter" rule on `string` keeps `--count 5` an `int`.
Conversely `--name 42` is an `int`, not a `string`: a value that looks
numeric is typed numeric, whatever the argument is called.

You never parse a raw value yourself. Consuming code cannot add a type
beyond this list today; see
[docs/concepts/instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md)
for why.

## Common mistakes

**Calling `GetRequiredArgument<T>` for something optional.** It throws
when the argument is missing. Use `GetArgument<T>` with a fallback
(`?? default`) for anything a user might reasonably omit.

**Asking for the wrong type.** `GetArgument<T>` finds only an argument the
rules above typed as `T`. `GetArgument<int>("page")` returns `null` in all
three of these cases:

- `--page` — no value, so it became a `bool`
- `--page abc` — has letters, so it became a `string`
- no `--page` at all

All three look alike to the factory. No separate signal distinguishes
"present but the wrong shape" from "absent."

**Reaching for `GetArgument` inside a command handler.** These helpers
live on `CliCommandFactory<T>` alone. By the time a handler runs, the
command instance holds whatever data it needs, and nothing remains to look
up.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — where a
  factory fits into a command, and when you need one.
- [docs/concepts/instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md) —
  how a raw ask becomes the typed arguments these helpers read, and the
  full list of built-in `IInstructionArgumentBuilder`s.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — `GetArtefact`
  and `GetRequiredArtefact`, the equivalents for state from *prior*
  commands in the same run.
