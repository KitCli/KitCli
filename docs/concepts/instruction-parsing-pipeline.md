# Instruction parsing pipeline

## Premise

A user invokes a KitCli command as one line of terminal input:

```
/spare-money help --argumentOne hello world --argumentTwo 1
```

Before any handler runs, that raw string must become a typed
`Instruction`: a prefix, a name, an optional sub-name, and a list of
typed arguments. `KitCli.Instructions` and its `.Abstractions` project
own that conversion. Nothing downstream sees the raw string again.

## Problem

A command handler needs its arguments as typed values, ready to use. It
should neither re-parse a raw string nor declare a schema up front.

Meeting that means turning one line of loose free text — a prefix, a
name, an optional sub-name, and any number of `--name value` pairs — into
a typed, addressable `Instruction`, with no per-command tokenizing and no
schema step to slow down adding a command.

## Solution

`InstructionParser.Parse` (`Parsers/InstructionParser.cs`) runs three
stages in order.

### 1. Indexing

`InstructionTokenIndexer.Index` (`Indexers/InstructionTokenIndexer.cs`)
locates four token regions — `Prefix`, `Name`, `SubName`, `Arguments` —
as start/end index pairs into the original string, allocating no
substrings.

Positional rules find each: the configured prefix character (default `/`)
at position 0, the first space ending the name, the configured argument
marker (default `--`) starting the arguments span, and whatever lies
between the name and the arguments as the sub-name.

### 2. Extraction

`InstructionTokenExtractor.Extract`
(`Extraction/InstructionTokenExtractor.cs`) turns those ranges into
strings.

The raw `Arguments` string then splits into a
`Dictionary<string, string?>`: first on the argument prefix (`--`) to
separate each `--name value` pair, then on the first space within each
pair to divide name from value. An argument written without a value
(`--verbose`) takes a `null` value, which matters for typing below.

One wrinkle: the indexer locates the arguments using the *configured*
argument prefix, while the extractor splits them using the hard-coded
`InstructionConstants.DefaultArgumentPrefix` (`--`). Configuring a
different prefix therefore fails halfway through.

### 3. Typing

Each raw value still needs a target type, and no per-command schema
supplies one. Instead every `IInstructionArgumentBuilder` (`Builders/`)
declares:

```csharp
bool For(string? argumentValue);                                                  // "can I claim this raw value?"
AnonymousInstructionArgument Create(string argumentName, string? argumentValue);  // convert it
```

For each argument, `InstructionParser.Parse` takes the **first**
registered builder whose `For` returns `true`. Registration order, set by
`ServiceCollectionExtensions.AddCliInstructionArgumentBuilders`
(`Extensions/ServiceCollectionExtensions.cs`), runs:

```
DirectoryInfo → Guid → String → Int → Decimal → DateOnly → Bool
```

That order is deliberate. Each builder's `For` is narrow enough to leave
the ones behind it their turn:

| Builder | Claims a value when |
|---|---|
| `DirectoryInfo` | it's a rooted path, or starts with `.` |
| `Guid` | `Guid.TryParse` succeeds |
| `String` | it contains at least one letter, and isn't `true`/`false` |
| `Int` | `int.TryParse` succeeds |
| `Decimal` | `decimal.TryParse` succeeds |
| `DateOnly` | `DateTime.TryParse` succeeds |
| `Bool` | always |

`String` sits ahead of the numeric builders, which is why it tests for a
letter. Without that test it would claim `--count 5`; with it, `5` falls
through to `Int`.

`BoolInstructionArgumentBuilder.For` returns `true` unconditionally. It
serves as the fallback, reached only because every builder ahead of it
gets first refusal. Its `Create` reads `true` or `false` when the value
parses as one and returns `true` otherwise, which is what makes a bare
`--verbose` flag work.

Order is the whole contract, not a convenience. Move a builder and every
value once claimed by the builder ahead of it silently changes type.

### Assembly

Each accepted `(name, typed value)` pair becomes an
`InstructionArgument<T>` (`Arguments/InstructionArgument.cs`), a record
deriving from the untyped `AnonymousInstructionArgument`.

`Parse` wraps the prefix, name, sub-name, and argument list into one
immutable `Instruction` record (`Instruction.cs`) — the single object
every command factory and handler downstream consumes. Its four
properties are `Prefix`, `Name`, `SubInstructionName` (the token regions
above call this `SubName`), and `Arguments`.

## Constraints & tradeoffs

**Type inference by testing each raw value, not by a declared schema.** No
handler declares that "argument `dueDate` is a `DateOnly`"; it asks for
the typed argument by name and gets it.

The cost: an argument's type reflects what its raw string looks like, not
which command was invoked, and the first `For()` returning `true` settles
it. That makes registration order load-bearing. Insert a builder in the
wrong position and existing commands' arguments change type, though those
commands never changed.

**No escaping or quoting grammar in the tokenizer.** Splitting on a fixed
`--` means that string can never appear inside a value without corrupting
extraction. Tracked as
[#39](https://github.com/KitCli/KitCli/issues/39).

**Culture-sensitive builders.** `IntInstructionArgumentBuilder`,
`DecimalInstructionArgumentBuilder`, and
`DateOnlyInstructionArgumentBuilder` parse with the current thread culture
— `int.Parse`, `decimal.Parse`, and `DateTime.Parse` with no `CultureInfo`
argument — rather than `CultureInfo.InvariantCulture`. The same input can
therefore yield a different value, or fail outright, depending on the host
machine's locale. Tracked as
[#22](https://github.com/KitCli/KitCli/issues/22).

## Questions & answers

**How do I add a new argument type?**
Consuming code has no supported extension point today.
`AddCliInstructionArgumentBuilders` is a private, fixed sequence of
`.AddSingleton<IInstructionArgumentBuilder, ...>()` calls inside
`KitCli.Instructions`. Adding a builder ahead of
`BoolInstructionArgumentBuilder` means editing that method directly, in
the right position.

Registering your own builder downstream cannot work.
`IServiceCollection` resolves `IEnumerable<T>` in registration order, so
anything added later lands behind Bool's unconditional `For`, and Bool
matches everything. Tracked as
[#9](https://github.com/KitCli/KitCli/issues/9).

**What happens if two builders could both handle a value?**
The one registered first wins outright. The second is never consulted,
and nothing raises an ambiguity error. Command factory resolution follows
the same first-match-wins rule; argument typing is not a special case.

When adding a builder, the question is not only whether your `For`
identifies your own type. Ask also whether it returns `true` for values a
builder ahead of it should own, because that builder never gets to
object.

**Where does an argument's name come from, if I want to look it up later?**
Whatever the user typed after `--`, verbatim, with no schema behind it. No
list of "arguments this command accepts" exists for the parser to check
against, and `DefaultInstructionValidator` checks only that a prefix and a
name are present.

A factory's later lookup (`GetArgument<T>(name)`, see
[artefacts.md](artefacts.md)) resolves only when the user typed that exact
`--name`. Get the name wrong on either side and nothing errors: the
argument, or the lookup, is silently absent.

## Related concepts

- [artefacts.md](artefacts.md) — `GetArgument<T>` and
  `GetRequiredArgument<T>` work like the artefact-lookup helpers, over an
  `Instruction`'s parsed arguments instead of the run's outcome history.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — where
  `InstructionParser.Parse`'s output is consumed, as the first step of
  `RespondToAsk`.
- [command-registration.md](command-registration.md) — how the parsed
  `Instruction.Name` becomes a command factory.
- [0004-first-match-wins-resolution.md](../adr/0004-first-match-wins-resolution.md) —
  why argument builder resolution and command and artefact factory
  resolution all follow one first-match-wins rule.
