# CLI I/O

## Premise

A KitCli app reads a line the user typed, writes lines back, sets the
window title, and learns when the user presses Ctrl+C. `ICliIo`
(`KitCli.Abstractions/Io/ICliIo.cs`) is the one seam through which all of
that passes, and `CliIo` beside it is the `Console`-backed default.

Nothing else in the framework touches `Console`. `CliApp` and its
subclasses hold an `ICliIo`; so does every `IOutcomeIoWriter`.

## Problem

Console access scattered through a framework is untestable and
unreplaceable. A host embedding KitCli in a GUI, a test asserting what a
command printed, and a terminal session all need the same commands to run
against different I/O.

Cancellation makes it harder. `Console.CancelKeyPress` fires on its own
thread, and .NET's default behavior is to kill the process — skipping
every `finally` block and `Dispose` the run depends on.

## Solution

The whole seam is five members:

```csharp
public interface ICliIo
{
    Task<string?> AskAsync(CancellationToken cancellationToken);
    void Pause();
    void Say(string something);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}
```

`AddCliAbstractions` registers `CliIo` as the singleton `ICliIo`. Its
implementations are thin:

| Member | `CliIo` does |
|---|---|
| `AskAsync` | races `Console.ReadLine` against the token |
| `Pause` | writes a blank line |
| `Say` | `Console.WriteLine(something)` |
| `SetTitle` | sets `Console.Title` |
| `OnCancel` | subscribes to `Console.CancelKeyPress` |

`Pause` writing a blank line matters, because callers rely on it for
spacing: `TerminalCliApp` calls it before its loop and after every
iteration, and `SuggestionOutcomeIoWriter` calls it before each suggestion
(see [outcome-writing.md](outcome-writing.md)). Another `ICliIo` may do
something else there — wait for a keypress, redraw a prompt.

### Cancelling a blocked read

`Console.ReadLine` cannot be cancelled once it blocks, so `AskAsync` does
not try:

```csharp
public async Task<string?> AskAsync(CancellationToken cancellationToken)
{
    var abandonableBackgroundConsoleRead = Task.Run(Console.ReadLine, CancellationToken.None);

    try
    {
        return await abandonableBackgroundConsoleRead.WaitAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        return null;
    }
}
```

The read runs on a background thread and the caller waits on the token.
Cancellation winning returns `null` and abandons the read where it stands,
still blocked, rather than waiting for a keypress that may never come.

### What a Ctrl+C does

```
Console.CancelKeyPress
  → CliIo sets e.Cancel = true          (suppresses .NET's abrupt exit)
      → the Action registered via OnCancel
          → Workflow.InterruptCurrentRun()
```

`CliApp.SetUpEventHandlers` registers that action once, at the top of
`Run`, and registers nothing else. The callback calls
`InterruptCurrentRun` and stops: no other workflow mutation, no
`OnSessionEnd`, no `Environment.Exit`. Setting `e.Cancel = true` first is
what keeps .NET's default termination from racing the app's own shutdown.

The token itself belongs to `ICliWorkflow`, not to `ICliIo` or `CliApp`
(see
[workflow-run-state-machine.md](workflow-run-state-machine.md)).
`AskAsync` is the one place a token crosses this seam, because sourcing an
ask is the one cancellable operation outside any run. See
[0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md)
for why cancelling always stops the workflow while stopping never cancels.

## Constraints & tradeoffs

**A cancelled `AskAsync` leaks a blocked thread.** The abandoned
`Console.ReadLine` holds a thread-pool thread until the user presses
Enter, or the process exits. For a session ending on Ctrl+C that costs
nothing, and it buys a prompt that gives up immediately.

**`null` means cancelled, and also means end-of-input.** `AskAsync`
returns `null` for both. `CliWorkflowRun.RespondToAsk` treats a null or
empty ask as `InvalidAsk` either way, so a piped input stream running dry
ends the run the same way a cancellation does.

**Output is synchronous and unstructured.** `Say` takes a string and
returns `void`; no async variant, no levels, no streams. A writer needing
structure formats it into the string first, as `TableOutcomeIoWriter` does
with `Table.ToString()`.

**One `ICliIo` for the whole app.** It registers as a singleton, so input
and output cannot be redirected per run or per command.

## Questions & answers

**How do I test what a command printed?**
Register your own `ICliIo` ahead of `AddCliAbstractions`, recording each
`Say` call. Because every writer and the host loop both go through the
seam, that captures all output without touching `Console`.

**Why does `Pause()` write a blank line rather than wait for a key?**
Callers use it for spacing, not for pacing — between loop iterations, and
before each suggestion. A custom `ICliIo` can make it wait, and the
default keeps a non-interactive run from blocking.

**Does a command handler get an `ICliIo`?**
It can, through constructor injection, but returning outcomes is the
better path. Outcomes reach a writer, stay testable, and let the host
decide the rendering (see [outcome-writing.md](outcome-writing.md)).

**Can a command handler be interrupted mid-run by Ctrl+C?**
Only if it observes the `CancellationToken` its handler receives.
Cancellation is cooperative; a handler ignoring the token runs to
completion. See
[0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md).

## Related concepts

- [cli-app-host.md](cli-app-host.md) — the loop calling `AskAsync` and
  `Pause`, and wiring `OnCancel` through `SetUpEventHandlers`.
- [outcome-writing.md](outcome-writing.md) — the eight writers that turn
  outcomes into `Say` calls.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — who
  owns the cancellation token, and what `InterruptCurrentRun` does to a
  run.
- [0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md) —
  why Ctrl+C drives a token rather than `Environment.Exit`.
