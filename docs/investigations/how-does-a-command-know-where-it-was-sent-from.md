# How could KitCli know where a command was sent from, and when to carry on?

- **Status:** In Review
- **Release:** Unreleased
- **Spike:** none filed — tests building the hand-written `ReturnTo` in
  [Asking for a missing value and carrying on](../user-guides/asking-for-a-missing-value-and-carrying-on.md)
  into KitCli
- **Time-box:** none agreed — ran inline
- **Date:** 2026-10-17

## Verdict

**New complexity.** KitCli needs new code to write down which command sent the
user to a question, and to send them back when the question says "move back".

A book shop app has an `/order` command, which needs a book to order. When
the user has not chosen one, `/order` sends them to Choose book, which asks
"Which book would you like?". Once they answer, the app should carry on with
`/order`. For that, Choose book must
know two things:

| Choose book must know | So that |
|---|---|
| **Where** it was sent from | It carries on with `/order`. Typed on its own, it just finishes. |
| **When** to carry on | It carries on only after a good answer. A bad answer asks again; cancelling stops. |

| Term | Meaning |
|---|---|
| Command | One step the app runs, such as `/order`. |
| Run | Every command from what the user typed up to the one that ends it. |
| Move | One command handing over to the next, written `ByMovingToCommand`. |
| Prior command | The command that moved the user to this one: `/order`, for Choose book. |
| Factory | Builds a command just before it runs. It can read notes earlier commands left. |

### Today

The app's developer builds both halves by hand. Choose book holds a `ReturnTo`
field, and one factory per sending command fills it in:

```mermaid
sequenceDiagram
    actor U as User
    participant E as /order
    participant C as Choose book
    U->>E: /order
    E->>C: move
    Note over C: the developer's factory for /order<br/>sets ReturnTo = /order
    C-->>U: Which book would you like?
    U->>C: Dune
    Note over C: the developer's handler checks<br/>ReturnTo is not null
    C->>E: move to ReturnTo
```

Every command that can send the user to Choose book needs its own factory.
Asking again needs another factory, which must copy `ReturnTo` across.

### After this work

KitCli writes a note when it follows a move: "Choose book was sent here by
`/order`." When Choose book says "move back", KitCli reads the note and runs
`/order` again:

```mermaid
sequenceDiagram
    actor U as User
    participant E as /order
    participant K as KitCli
    participant C as Choose book
    U->>E: /order
    E->>K: move to Choose book
    Note over K: note: Choose book was sent by /order
    K->>C: run
    C-->>U: Which book would you like?
    U->>C: Dune
    C->>K: move back
    Note over K: reads the note
    K->>E: run again
```

The developer's code loses the `ReturnTo` field, the per-sender factories and the
`null` check. `/order` is unchanged. The names below are proposals, not on
`main`:

```csharp
// Choose book's handler, after a good answer
return FinishThisCommand()
    .ByResultingIn(new ChosenBookOutcome(chosenBook))
    .ByMovingBackToPriorCommand()
    .End();

// Choose book's factory, to word a message such as "Back to your order"
public override CliCommand Create()
    => new ChooseBookCliCommand(BackToOrder: WasSentFrom<OrderCliCommand>());
```

KitCli handles each case:

| What happens | What KitCli does |
|---|---|
| The user types Choose book on its own. | No note is written, so `ByMovingBackToPriorCommand()` does nothing and the command just finishes. |
| Choose book asks again by moving to itself. | It keeps the note it already has, instead of writing "sent by Choose book". |
| Choose book moves back. | It sends the user back without writing a new note. |
| The user cancels. | Choose book ends the run as it does today, and the note goes with it. |

## Recommendation

Three delivery tickets, in this order, as one minor release. An ADR comes with
the first, because the note is a new pattern every command can use.

**1. Write the "sent by" note when following a move, and add `WasSentFrom<T>()`.**

**2. Add `ByMovingBackToPriorCommand()`.** It sends the user back to the command in the note.

**3. Rewrite the asking guide around the two.**

## What was established

| Fact | Consequence |
|---|---|
| The run can already tell a typed command from a moved-to one. | KitCli knows when to write a note and when not to. |
| Factories already read earlier notes, such as the record of which command ran. | `WasSentFrom<T>()` can read the new note the same way. |
| The guide's factory checks only which command ran last. | Typing Choose book straight after `/order`, in the same run, wrongly sends the user back. The note fixes this. |
| `/order` runs again from the top, rebuilt by its factory. | A typed flag such as `--gift-wrap` is lost unless remembered first. |

## Evidence

Read against `eec501a`.

| File | Shows |
|---|---|
| `docs/user-guides/asking-for-a-missing-value-and-carrying-on.md:35-65` | The hand-written `ReturnTo` field, per-sender factory and `null` check. |
| `KitCli.Workflow/Run/CliWorkflowRun.cs:120` | A typed command is recorded with what the user typed. |
| `KitCli.Workflow/Run/CliWorkflowRun.cs:136` | A moved-to command is recorded without it. |
| `KitCli.Workflow/Run/CliWorkflowRun.cs:164` | Every command that runs is recorded. |
| `KitCli.Workflow/Run/CliWorkflowRun.cs:214` | A moved-to command is built from a blank instruction, which is why typed flags are lost. |
| `KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs:46` | Factories read earlier outcomes turned into notes. |
| `KitCli.Commands.Abstractions/Factories/CliCommandFactory.cs:162` | `LastCommandIs<T>()`, the check the guide's factories use today. |

## Open questions

- Should asking again be its own call, such as `ByAskingAgain()`, or any move
  from a command to itself?
- When KitCli runs `/order` again, should it replay what the user first typed,
  so flags such as `--gift-wrap` survive?

## Out of scope

- **A question that leads to another question.** It needs more new code:
  [Carrying on after nested questions](what-would-it-take-to-carry-on-after-nested-questions.md).
