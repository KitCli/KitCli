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

One line of input mixes several kinds of content with no fixed grammar
declared anywhere: a marker that says "this is a command" (the prefix),
the command's own name, an optional sub-command name, and an arbitrary
number of `--name value` argument pairs. And no command declares a
schema for what its arguments should look like — so once an argument's
raw text is captured, something still has to decide whether
`--dueDate 2024-01-01` is a date, a string, or a number.

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
bool For(string? argumentValue);                            // "can I claim this raw value?"
AnonymousInstructionArgument Create(string name, string? value);  // convert it
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

**Type inference by sniffing content, not by a declared schema.** A
command handler never has to declare "argument `dueDate` is a
`DateOnly`" anywhere — it just asks for the typed argument by name and
gets it. The cost is that an argument's type is entirely a property of
what its raw string looks like, decided by whichever builder's `For()`
returns `true` first — not a property of the command being invoked. This
is why builder registration order is load-bearing: inserting a new
builder in the wrong position changes the resolved type of existing
commands' arguments without touching those commands at all.

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
Implement `IInstructionArgumentBuilder` and register it in
`AddCliInstructionArgumentBuilders`, in a position *before*
`BoolInstructionArgumentBuilder` and before any existing builder whose
`For` would also incorrectly match your new type's typical raw values.
Where exactly you insert it determines which raw values it actually gets
to claim.

**What happens if two builders could both handle a value?**
The first one registered wins outright — the second is never consulted,
and there's no ambiguity error raised. This mirrors the same
first-match-wins, registration-order-decides pattern used for command
factory resolution elsewhere in KitCli; it isn't unique to argument
typing.

**Where does an argument's name come from, if I want to look it up later?**
The key produced during extraction (e.g. `argumentOne` from
`--argumentOne`) becomes `AnonymousInstructionArgument.Name` — the same
name a `CliCommandFactory<T>`'s `GetArgument<T>(name)` helper (see
[outcome-artefact-pipeline.md](outcome-artefact-pipeline.md)) looks up
by, later in the pipeline.
