# 0007. Should an instruction argument's type come from its content or the command's declaration?

- **Status:** In Review
- **Spike:** [#8](https://github.com/KitCli/KitCli/issues/8)
- **Time-box:** none agreed — the estimate was removed when #8 became a spike
- **Date:** 2026-08-29

## Verdict

New complexity. Declaration wins, but not in the shape #8 proposed, and
not as one ticket.

Both repros are real. With the seven builders in production registration
order, `/spend --amount .5` binds as
`InstructionArgument<DirectoryInfo>` and `--reference 0012` becomes
`int 12`, the leading zero gone. `0.5` binds fine — the bug is
content-dependent, which is exactly the problem: what a value *is* cannot
be read off what it *looks like*.

#8 proposed an up-front schema: each command declares its argument names
and CLR types, and the parser binds against them. That shape cannot work
here. Arguments are typed inside `InstructionParser.Parse`, before any
factory is chosen — and choosing a factory can depend on the arguments,
because `CanCreateWhen()` may call `AnyArgument<T>`. A schema keyed by
command is consulted after selection, but selection is what it would need
to inform. Unioning the declarations of every factory under a name breaks
the moment two of them declare the same argument differently.

The declaration KitCli needs already exists, twice. Every retrieval site
names both halves — `GetRequiredArgument<decimal>("amount")` — and the
chain path already builds arguments from declared types with no
inference at all (`NextCliCommandArgument<TValue>.ToInstructionArgument()`).
Today the same factory receives `decimal` from a chained hop and
`DirectoryInfo` from the terminal for the same value. The framework
already disagrees with itself, and the declared side is the correct one.

So the fix is: parse keeps every argument as its raw text, and the
retrieval helpers convert to the type the caller asked for, failing
loudly when the text will not convert. That deletes the builder chain —
a published extension point — reverses one of
[ADR 0004](../adr/0004-first-match-wins-resolution.md)'s four points,
changes what run history records, needs a converter registry and a live
error code, rewrites a concept doc and a user guide, and lands as a major
bump. It also settles #9 and #40 outright, fixes #22 as a side effect,
and brings part of #21's dead taxonomy to life. That is several tickets
and an ADR, not the sized-13 bug fix #8 started as.

## Recommendation

#8 closes once the breakdown below is agreed. Build in this order.

1. **Cover `CliCommandFactory`'s retrieval helpers**
   ([#114](https://github.com/KitCli/KitCli/issues/114)) — the contract
   that changes, already the first step of the descriptor build
   (investigation 0006). One prerequisite, shared by both workstreams.
2. **`feat(instructions)`: a converter registry** — additive.
   `IInstructionArgumentConverter` chosen by *target CLR type*, not by
   content, so there is no ordering and nothing to shadow. Ship the
   seven built-ins, pinned to `CultureInfo.InvariantCulture` with
   explicit styles (closes the mechanism behind #22). A null raw value
   requested as `bool` converts to `true` — the flag idiom
   `--dry-run` must survive.
3. **`feat(instructions)!`: type at retrieval, not at parse** — the
   breaking ticket. `InstructionParser.Parse` emits a raw argument
   (`RawInstructionArgument(string Name, string? Value)` under
   `AnonymousInstructionArgument`); `GetArgument<T>` returns a
   chain-typed `InstructionArgument<T>` as-is, otherwise converts the
   raw text through the registry. Failure throws `InstructionException`
   with a new `ArgumentNotConvertible` code naming the argument, its
   text, and the requested type; `GetRequiredArgument<T>`'s bare
   `Exception` becomes `ArgumentIsRequired`, making #21's codes real.
   Delete the seven builders and `IInstructionArgumentBuilder`. The ADR
   superseding ADR 0004's parser row rides in this PR, as do the
   rewrites of [concept 0005](../concepts/0005-instruction-parsing-pipeline.md)
   and [user guide 0005](../user-guides/0005-reading-command-arguments.md)
   and the `CHANGELOG.md` breaking entry. If the file count passes 20,
   split the doc rewrites out, nothing else.
4. **Settle the satellites** — #9 and #40 close as moot (no builder
   chain to be unreachable or untested); #22 closes on verification;
   #21 is re-scoped to whatever stays dead.
5. **Coordinate with the descriptor build**
   ([#190](https://github.com/KitCli/KitCli/issues/190),
   [#191](https://github.com/KitCli/KitCli/issues/191)) —
   `RequiresArgument<T>` should test convertibility through the same
   registry, and `--help` can render the declared types. Same registry,
   two consumers.

## What was established

**Typing happens before the command exists, and cannot be moved after
it.** `CliWorkflowRun.RespondToAsk` parses at line 92 and resolves the
command at line 104; between them sits the validator. Factory selection
reads the typed arguments (`Attach`, then `CanCreateWhen`), so any
scheme that needs the factory before it can type the arguments is
circular. Conversion at retrieval is the only place a per-command
declaration and a raw value can meet without restructuring the run.

**The chain path is already declaration-typed.**
`NextCliCommandArgument<TValue>` creates `InstructionArgument<TValue>`
from the type the handler wrote, no inference. Retrieval-time conversion
must pass these through untouched, which the as-is check in step 3
covers.

**Consumers already pay for content typing at every money argument.**
SpendfulnessCli's `InstructionArgumentExtensions.OfCurrencyType` probes
`int` then `decimal`, because `--amount 5` infers `int` and
`--amount 5.5` infers `decimal`. Under declared retrieval,
`GetRequiredArgument<decimal>("amount")` answers both and the workaround
dies.

**Deleting the builder chain breaks no working consumer.** #9
established that `BoolInstructionArgumentBuilder.For` returns `true`
unconditionally and sits ahead of any builder a consumer appends after
`AddCliInstructions()`, so the extension point is unreachable from
consuming code. Still a public-surface break: packages version in
lockstep ([ADR 0015](../adr/0015-version-the-packages-in-lockstep.md)),
so the set takes a major.

**Raw history is truer history.** Run state records the instruction
(`InstructionCliWorkflowRunStateChange`), so today it stores parse-time
types — which #22 shows are locale-dependent. Raw text plus
invariant-culture conversion makes a recorded run mean the same thing on
every host.

**ADR 0004's other three points stand.** Command factories, artefact
factories, and outcome writers resolve a *handler* for a value that
genuinely exists; the parser was minting a *type* for one. Only the
parser row is superseded.

## Evidence

- Throwaway NUnit fixture in `KitCli.Instructions.Tests`, all seven
  builders in the order `AddCliInstructionArgumentBuilders` registers
  them: `.5` → `InstructionArgument<DirectoryInfo>`, `0012` →
  `InstructionArgument<int>` value `12`, `0.5` →
  `InstructionArgument<decimal>`. Deleted after the run.
- `KitCli.Instructions/Parsers/InstructionParser.cs:42-44` —
  `.First(builder => builder.For(token.Value))`.
- `KitCli.Instructions/Extensions/ServiceCollectionExtensions.cs:29-37`
  — the registration order.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:92,104` — parse before
  resolve; `:207-213` — the chain path building declared-type arguments.
- `KitCli.Commands.Abstractions/Factories/CliCommandFactory.cs:94-106`
  — `GetRequiredArgument<T>` and its bare `Exception`; `:214-220` —
  retrieval is `OfType<InstructionArgument<T>>()`, which is why a
  mis-typed argument reads as missing.
- `KitCli.Commands.Abstractions/Arguments/NextCliCommandArgument.cs:17-18`
  — `ToInstructionArgument`.
- `KitCli.Instructions.Abstractions/InstructionExceptionCode.cs` —
  `ArgumentIsRequired` exists and nothing throws it (#21).
- `SpendfulnessCli.Commands.Reporting/InstructionArgumentExtensions.cs`
  — `OfCurrencyType`.
- `KitCli.Tooling.Release/ReleaseCliCommand.cs:16-17` — the flag idiom
  step 2 must preserve.

## Open questions

- `GetArgument<T>(null)` — "the last argument of that type, any name" —
  has no meaning over raw text, where every value converts to `string`.
  Either the name becomes required or null means "the last argument"
  outright. [User guide 0005](../user-guides/0005-reading-command-arguments.md)
  currently blesses the null form for single-argument commands.
- A chain-typed `InstructionArgument<int>` retrieved as
  `GetArgument<decimal>`: convert through the registry, or miss? Decide
  in step 3's ticket.
- Does `DirectoryInfo` stay a built-in converter target, now that a path
  is only a `DirectoryInfo` when a factory asks for one?

## Out of scope

- Quoting and escaping in the tokenizer (#39) — same milestone
  neighborhood, different stage of the pipeline.
- Rebuilding validation on FluentValidation (#183).
- The descriptor build itself (#189, #190, #191), past sharing the
  registry.
- Most-specific-match for ADR 0004's other three points, out of scope
  here exactly as it was in investigation 0006.
