# Command stacking

**Command stacking builds one task from small commands, one per step.** Each
command [chains](../user-guides/chaining-commands.md) to the next and passes
on what it decided.

| Term | Meaning |
|---|---|
| Command | One step the app can run, such as `/export`. |
| Handler | Does a command's work. |
| Outcome | What a handler returns: a message to show, a value to remember, or the command to run next. |
| Artefact | A remembered outcome that any later command can read. |
| Factory | Builds a command, reading artefacts to fill it in. |
| Run | Every command from the first one up to a final outcome. |

## How it works

An export needs to know where to read from, its **source**. The user chooses
it once. Then the task connects, checks, writes and summarises:

```mermaid
flowchart LR
    Ask(["user types /export"]) --> Export["/export"]
    Export -->|"no source, may ask"| Choose[Choose a source]
    Choose -->|source chosen| Export
    Choose -->|nothing chosen| End1[["Ends: no source"]]
    Export -->|"no source, told not to ask"| End1
    Export -->|has a source| Connect[Connect]
    Connect --> Check[Check]
    Check -->|fails| End2[["Ends: check failed"]]
    Check -->|passes| Write[Write]
    Write --> Summarise[Summarise]
    Summarise --> End3[[Ends]]
```

Every box from `/export` to Summarise is its own command. The names are
examples, not part of KitCli.

In `/export`'s handler, remember the source and name the next command. KitCli
runs that command next:

```csharp
return FinishThisCommand()
    .ByResultingIn(new SourceOutcome(command.Source))
    .ByMovingToCommand<ConnectCliCommand>()
    .EndAsync();
```

In the next command's factory, read the source back:

```csharp
public override CliCommand Create()
    => new ConnectCliCommand(GetRequiredArtefact<string>(nameof(SourceArtefact)).Value);
```

`SourceOutcome`, `SourceArtefact` and `ConnectCliCommand` are the app's own
types. [Remembering your own state](../user-guides/remembering-your-own-state.md)
shows how to write the first two.

## The rule people miss

**Only the command the user typed can read what they typed.** A command the
run moves on to sees only the artefacts and any arguments the previous handler
passes it. To let every later command see a flag such as `--dry-run`, the first
command returns it as an outcome.

**Services do the work; commands make the decisions.** A service reaches
something outside the run: a database, an API, settings.

| Fact | Consequence |
|---|---|
| A choice kept in a service outlives the run that made it. | A later run can act on a stale choice. |
| An artefact ends with its run, unless its outcome has `OutcomeLifetime.Session`. | A session outcome carries into every later run through the `Created → Created` change. |

| Put it in a service | Put it in a command |
| --- | --- |
| Reading and writing data | Choosing which source to use |
| Calling an API | Deciding which command runs next |
| Loading settings | Passing a choice to later commands |

## Gaps

- [Nothing catches a chain that hands on forever (#173)](https://github.com/KitCli/KitCli/issues/173).
  A command that keeps moving on, and never ends the run, never returns.

## See also

- [Artefacts](artefacts.md): how a decision is read back by a later command.
- [Outcomes](outcomes.md): the lifetimes that decide how long a decision lasts.
- [Asking for a missing value and carrying on](../user-guides/asking-for-a-missing-value-and-carrying-on.md): the ask step of the export chain.
