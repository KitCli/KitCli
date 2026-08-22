# Reading command arguments

## What this is for

A user's ask can carry `--name value` pairs — `/greet --name Alex`.
Getting those values, already typed (not raw strings you parse
yourself), into the command you construct is what a command factory's
argument helpers are for.

## How to do it

Inside a `CliCommandFactory<T>`, read arguments by their declared type
and name:

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

`/greet --name Alex --shout` resolves `name` to `"Alex"` and `shout`
to `true` — a bool argument written with no value at all (a bare
flag) resolves to `true`, the common CLI flag idiom. `/greet --name
Alex` (no `--shout` at all) leaves it unset, so the `?? false` default
applies.

Three helpers are available, all generic over the argument's target
type:

```csharp
protected InstructionArgument<T>? GetArgument<T>(string? argumentName);
protected InstructionArgument<T> GetRequiredArgument<T>(string? argumentName);  // throws if missing
protected bool AnyArgument<T>(string? argumentName);
```

Passing `null` for `argumentName` matches the last argument of that
type regardless of name — useful only when a command genuinely takes
one argument of that type; prefer naming arguments explicitly
otherwise.

### Supported argument types

Out of the box: `DirectoryInfo`, `Guid`, `string`, `int`, `decimal`,
`DateOnly`, `bool`. Whatever raw string the user typed for
`--name value` is matched against that list, in that order, by the
**first** type it's valid for — so `--count 5` becomes an `int`
argument, not a `string`, even though `"5"` could technically be
either. `bool` is the fallback: anything no earlier type claims
becomes a `bool` (see the flag idiom above). You never parse the raw
value yourself. If you need a type that isn't in this list, that's a
new `IInstructionArgumentBuilder`; see
[docs/concepts/instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md)
for how the built-in ones work.

## Common mistakes

**Calling `GetRequiredArgument<T>` for something genuinely optional.**
It throws when the argument is missing — use `GetArgument<T>` with a
fallback (`?? default`) for anything the user might reasonably omit.

**Asking for the wrong type for what the user actually typed.**
`GetArgument<T>` only finds an argument that was *typed as* `T` by
the pipeline above — `GetArgument<int>("page")` returns `null` for
`--page` (no value, so it becomes `bool`), for `--page abc` (which
becomes `string`, not `int`), and for a genuinely missing `--page`.
All three look identical to the factory: there's no separate signal
for "present but the wrong shape" versus "not present at all."

**Reaching for `GetArgument` inside a command handler.** These helpers
only exist on `CliCommandFactory<T>` — by the time a handler runs, the
command instance already has whatever data it needs; there's nothing
left to look up.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — where a
  factory fits into a command overall, and when you need one at all.
- [docs/concepts/instruction-parsing-pipeline.md](../concepts/instruction-parsing-pipeline.md) —
  how a raw ask string becomes the typed arguments these helpers read,
  and the full list of built-in `IInstructionArgumentBuilder`s.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — the
  equivalent helpers (`GetArtefact`, `GetRequiredArtefact`) for reading
  state from *prior* commands in the same run, rather than the current
  ask's arguments.
