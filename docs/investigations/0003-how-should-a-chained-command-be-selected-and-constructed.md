# 0003. How should a chained command be selected and constructed?

- **Status:** In Review
- **Spike:** #148
- **Time-box:** 30 minutes
- **Date:** 2026-08-25

## Verdict

New complexity. #147 asks for one method. It needs four tickets and an ADR.

Some background, because the question is hard to state without it. A handler can
end by handing straight on to another command, so one thing the user typed runs
several commands in a row. That is a chain. Today the handler builds the next
command itself, with `new`. Dan's complaint is that this skips the factory.

Factories matter because of what they can see. When a command comes from
something the user typed, KitCli asks a factory to build it, and that factory
can read everything the run has gathered so far. A command built with `new` in a
handler gets none of that. It only gets whatever that one handler happened to
have to hand. So the same command is built two different ways depending on how
it was reached, and only one of those ways can see the run.

Every design question this raised now has an answer, and they are all below.
That is worth saying clearly: nothing here is unresolved. But answering them did
not make the work smaller. Building it means a new kind of outcome, a shared
base type over two of them, a new method on an interface that is already
published, changes at three places in `CliWorkflowRun`, four rewritten guides,
and #124 rewritten around a rule it does not currently propose. No single piece
is hard. There are a lot of pieces.

Two things also have to happen first. #11 means `MoveToNext()`'s main path has
no test, and every ticket below edits that path. #142 has to land first, because
factories are currently registered as singletons while holding state that
changes per command.

## Recommendation

Break #147 into four tickets, in this order. All of them wait on #142.

1. Test `MoveToNext()`'s happy path, which is the half of #11 still outstanding.
   Everything below edits that path, and nothing covers it today.
2. Add `ByMovingToCommand<TCommand>()`. The handler names the command type; the
   factory builds it when the run gets there. Details in the findings below.
3. Rewrite #124's selection rule around the queued outcome rather than the
   command, and settle the multiple-hop guard in the same ticket. They are one
   decision.
4. Rewrite `0007-chaining-commands.md`, `0008-artefacts.md`, `0006-outcomes.md`
   and `0010-workflow-run-state-machine.md` to teach the new method first.

Ticket 2 needs an ADR. It changes the shape of published API, and someone will
later ask why there are two ways to queue a command.

## What was established

**A chained hop that cannot be built is a bug, and the run already has somewhere
to put it.**

When a user types something that matches no command, KitCli offers them the
commands that would have worked. That is `SuggestNextCommands`, and it is a
kindness to a person who guessed wrong. A chain is not a person guessing. An
engineer wrote the hop into the code. If the factory cannot build it, the code is
wrong, and the useful thing to do is fail loudly.

The run already handles that. `ExecuteCommand` wraps everything in a try/catch
and moves the run to `Exceptional` on any failure, and `Running → Exceptional` is
already a legal move. So no new state is needed. The one constraint this puts on
the implementation is that resolution has to happen inside that try block, so the
failure is caught like any other.

**The new outcome should carry a type and nothing else.**

An outcome is how a handler says what happened. One of them,
`NextCliCommandOutcome`, currently means "run this command next" and carries the
finished command. The new one means "run this *kind* of command next" and carries
only the type. Nothing is built when the hop is queued. The factory builds it
when the run arrives, and the existing `RanCliCommandOutcome` then records what
was built. This is the whole point: an instance stored on the outcome is an
instance the previous handler built, which is the thing being removed.

**It has to be a sibling of the existing outcome, not a subclass.**

`NextCliCommandOutcome` requires a command in its constructor. A subclass would
have to invent one to satisfy that, which defeats the purpose. So both outcomes
should sit under a shared base type instead. The cost is small: three places in
`CliWorkflowRun` currently look for `NextCliCommandOutcome` by name and would
look for the base type instead — `MoveToNext`, `IsValidMovePastAsk` and
`UpdateStateAfterOutcome`.

**Adding an outcome to this family is safe, which is not true everywhere.**

Outcomes are printed by writers, and `CliApp` picks the first writer that claims
one. #106 records the danger: if one outcome inherits from another, a writer for
the parent can swallow the child and print the wrong thing. That cannot happen
here, because no writer claims `NextCliCommandOutcome` at all. It is an
instruction to the run, not something shown to the user, and `WriteOutcomes`
silently skips anything no writer claims.

**Track what has already run against the queued outcome, not the command.**

#124 wants a handler to queue more than one hop. That needs a way to tell which
hops have already run. #124 proposes matching against `RanCliCommandOutcome`,
which records the command. That works while every hop carries a finished command,
and stops working once a hop carries only a type — two hops to the same kind of
command become indistinguishable, so a chain that visits a step twice either
repeats it or stalls.

Matching on the queued outcome avoids this. Every `ByMovingToCommand` call
creates its own outcome object, so two hops to the same kind of command are still
two different objects. The rule then works the same for both kinds of hop.

**Everything the factory needs is already reachable.**

A factory is handed the instruction and the run's gathered data before it builds
anything. Both are available at the point a chain moves on. The instruction is
recorded in the run's own history, through
`IInstructionCliWorkflowRunStateChange`. The gathered data is already assembled
by `CliWorkflowCommandProvider` for the normal path. Provided the new resolution
happens inside that provider rather than in `CliWorkflowRun`, nothing new has to
be exposed to reach either.

**The factory can be found from the type using what already exists.**

`CliCommand.GetInstructionName()` and `StripCommandName` already turn a command
type's name into the name it is registered under. Nothing new is needed to look
one up. Factories are also registered under a shorthand and under any aliases,
and those can collide — but a lookup by the derived full name is not affected by
that, and where two command types genuinely share a name the ambiguity is #19's,
not this feature's.

**A command with constructor arguments and no factory of its own cannot be
chained this way, and no compiler check will catch it.**

KitCli only auto-registers a factory for a command it can build with `new`. A
command taking constructor arguments needs a factory written for it, or it has
none at all. Constraining the new method to types with an empty constructor would
exclude exactly the commands this feature is for, so the check cannot happen at
compile time. Per the first finding, failing at runtime is the right answer.

**Adding to the provider's interface need not break anyone.**

`ICliWorkflowCommandProvider` ships in a published package, so adding a method to
it would normally break anyone who has written their own. C# has a feature for
this — a default implementation on the interface, which existing implementations
inherit without changing. Every project targets `net10.0`, so it is available.
Have the default throw; per the first finding, that surfaces as `Exceptional`
like any other failure. The change then stays additive and the package takes an
ordinary patch bump, which is all the release tooling can produce anyway (#127).

The alternative — a second interface, resolved where the run is built — would
add a parameter to `CliWorkflowRun`'s public constructor and break eight test
call sites. Worse, for the same result.

## Evidence

- `KitCli.Workflow/Run/State/CliWorkflowRunState.cs:117-139` — the legal state
  moves, including `Running → Exceptional`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:143-166` — `ExecuteCommand` catching
  everything and returning an `ExceptionOutcome`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:139`, `:174-176`, `:199` — the three
  places that look for `NextCliCommandOutcome`.
- `KitCli.Commands.Abstractions/Io/*.cs` — every `CanWriteFor` tests one exact
  type, and none of them tests `NextCliCommandOutcome`.
- `KitCli/CliApp.cs:50-59` — `WriteOutcomes` skipping outcomes no writer claims.
- `KitCli.Commands.Abstractions/CliCommand.cs:19-36` — `GetInstructionName` and
  `StripCommandName`.
- `KitCli.Commands.Abstractions/Outcomes/Reusable/RanCliCommandOutcome.cs` —
  records the built command.
- `KitCli.Workflow.Abstractions/Run/State/Change/IInstructionCliWorkflowRunStateChange.cs`
  — exposes the instruction publicly.
- `KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs:22-58` — factory
  lookup, and the private assembly of the run's gathered data.
- `KitCli.Commands.Abstractions/Extensions/CommandServiceCollectionExtensions.cs:66-71`
  — auto-registration skipping commands without an empty constructor.
- `KitCli.Workflow/CliWorkflow.cs` — `CreateNewRun` building the run;
  `new CliWorkflowRun(` appears at eight test call sites.
- `KitCli.Workflow.Commands/KitCli.Workflow.Commands.csproj:11-12` — published
  package, version 1.0.10; `net10.0` throughout.
- [Safely update interfaces using default interface methods](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions).

## Open questions

None about the design. Three arose during the spike and all three closed: both
overloads survive with the guides leading on the new one, the multiple-hop guard
is #124's decision and belongs in the run, and the interface gains its method
through a default implementation.

What remains is procedural. CONTRIBUTING describes what to do when a spike finds
the work is as small as expected. It does not describe this case.

## Out of scope

- `[CliNextCommandIs]` and `SuggestNextCommands`, past establishing that a chain
  does not need them.
- #124's other idea, an attribute that documents a chain without changing how it
  runs.
- `ActivatorUtilities`, rejected on #147 before the spike because it skips the
  factory, which is the thing being asked for.
- Bright.DataTool.Cli, the app that prompted both issues.
