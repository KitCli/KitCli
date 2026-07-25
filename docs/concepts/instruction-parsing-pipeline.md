# Instruction parsing pipeline

## Premise

A KitCli command is invoked as one line of terminal input, e.g.

```
/spare-money help --argumentOne hello world --argumentTwo 1
```

Before any command handler runs, that raw string has to become a typed
`Instruction` — a prefix, a name, an optional sub-name, and a list of
typed arguments. `KitCli.Instructions` (and its `.Abstractions` project)
owns turning free text into that structure; nothing downstream ever
looks at the raw string again.

## Problem

A command handler needs its arguments as ready-to-use typed values — not
a raw string it re-parses itself, and not according to a schema it has
to declare up front for every command. Solving that means turning one
line of loosely-structured free text (a prefix, a name, an optional
sub-name, and an arbitrary number of `--name value` pairs) into a typed,
addressable `Instruction`, without hand-rolling tokenizing/type-conversion
logic per command, and without a schema-declaration step slowing down
adding a new one.

## Solution

`InstructionParser.Parse` (`Parsers/InstructionParser.cs`) runs three
stages in order:

### 1. Indexing

`InstructionTokenIndexer.Index` (`Indexers/InstructionTokenIndexer.cs`)
locates four token regions as start/end index pairs into the original
string, without allocating any substrings yet: `Prefix`, `Name`,
`SubName`, `Arguments`. It looks for `InstructionSettings.Prefix`
(default `/`) at position 0, the first space to end the `Name` token,
`InstructionSettings.ArgumentPrefix` (default `--`) to start the
`Arguments` span, and treats everything between the end of `Name` and
the start of `Arguments` as `SubName`.

### 2. Extraction

`InstructionTokenExtractor.Extract` (`Extraction/InstructionTokenExtractor.cs`)
turns those index ranges into actual strings, then further splits the
raw `Arguments` span into a `Dictionary<string, string?>` by splitting on
the argument prefix (`--`) and, within each chunk, on the first space to
separate the argument's name from its value.

### 3. Typing

Each argument's raw string value still needs a target type. There is no
per-command schema for this — instead, every `IInstructionArgumentBuilder`
(`Builders/`) declares:

```csharp
bool For(string? argumentValue); // "can I claim this raw value?"
AnonymousInstructionArgument Create(string name, string? value); // convert it
```

`InstructionParser.Parse` picks the **first** registered builder whose
`For` returns `true` for each argument's raw value. Registration order,
from `ServiceCollectionExtensions.AddCliInstructionArgumentBuilders`
(`Extensions/ServiceCollectionExtensions.cs`), is:

```
DirectoryInfo → Guid → String → Int → Decimal → DateOnly → Bool
```

`BoolInstructionArgumentBuilder.For` unconditionally returns `true` — it
is the fallback that only ever fires because every builder ahead of it
gets first refusal. Order here isn't a convenience, it's the entire
contract: move a builder, and every argument value that used to be
claimed by whatever was ahead of it starts being claimed by the new
builder instead, silently.

### Assembly

Each accepted `(name, typed value)` pair becomes an
`InstructionArgument<T>` (`Arguments/InstructionArgument.cs`, a record
deriving from the untyped `AnonymousInstructionArgument`). `Parse` then
wraps the prefix, name, sub-name, and argument list into one immutable
`Instruction` record (`Instruction.cs`) — the single object every
command factory and handler downstream actually consumes.

## Constraints & tradeoffs

**Type inference by testing the raw value against each builder in turn,
not by a declared schema.** A command handler never has to declare
"argument `dueDate` is a `DateOnly`" anywhere — it just asks for the
typed argument by name and gets it. The cost is that an argument's type
is entirely a property of what its raw string looks like, decided by
whichever builder's `For()` returns `true` first — not a property of the
command being invoked. This is why builder registration order is
load-bearing: inserting a new builder in the wrong position changes the
resolved type of existing commands' arguments without touching those
commands at all.

**No escaping/quoting grammar in the tokenizer.** Splitting on a fixed
argument prefix string (`--`) means that string can never appear
literally inside an argument's value without corrupting extraction —
this is a known, tracked gap, not an oversight to route around locally.

**Culture-sensitive builders.** `IntInstructionArgumentBuilder`,
`DecimalInstructionArgumentBuilder`, and `DateOnlyInstructionArgumentBuilder`
all parse with the current thread culture (`int.Parse`, `decimal.Parse`,
`DateTime.Parse` with no `CultureInfo` argument), not
`CultureInfo.InvariantCulture` — the same input can parse to a different
value, or fail to parse at all, depending on the host machine's locale.
Also a known, tracked gap.

## Questions & answers

**How do I add a new argument type?**
There's no supported extension point for this from consuming code today.
`AddCliInstructionArgumentBuilders` is a private, fixed sequence of
`.AddSingleton<IInstructionArgumentBuilder, ...>()` calls inside
`KitCli.Instructions` itself — the only way to add a builder ahead of
`BoolInstructionArgumentBuilder` is to edit that method directly, in the
right position. Registering your own `IInstructionArgumentBuilder` from
downstream consuming code doesn't achieve this: `IServiceCollection`
resolves `IEnumerable<T>` in registration order, so anything added
afterwards lands *after* Bool's unconditional `For` — and since Bool
matches everything, a builder registered behind it would never be
reached.

**What happens if two builders could both handle a value?**
The first one registered wins outright — the second is never consulted,
and there's no ambiguity error raised. This mirrors the same
first-match-wins, registration-order-decides pattern used for command
factory resolution elsewhere in KitCli; it isn't unique to argument
typing. If you're adding a builder (per the answer above), the thing to
check isn't just "does my `For` correctly identify my own type" — it's
"does my `For` also accidentally return `true` for values a builder
ahead of it was supposed to own," since that builder never gets a
chance to object.

**Where does an argument's name come from, if I want to look it up later?**
Whatever the user actually typed after `--` in the terminal input —
verbatim, with no declared schema behind it. There's no list anywhere of
"arguments this command accepts" for the parser to check against; the
name a command factory looks up later (via `GetArgument<T>(name)`, see
[outcome-artefact-pipeline.md](outcome-artefact-pipeline.md)) only
resolves if the user happened to type that exact `--name`. Get the name
wrong on either side — the terminal input or the lookup call — and
nothing errors; the argument, or the lookup, is just silently absent.
