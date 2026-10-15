# CLI I/O

A framework that calls `Console` directly cannot be tested and cannot be
hosted anywhere else. So KitCli calls it in exactly one class. `ICliIo` is
the seam: everything a KitCli app reads, writes, or learns about Ctrl+C
goes through it, and nothing else in the framework touches `Console`.

```csharp
public interface ICliIo
{
    bool CanAsk => true;   // HeadlessCliIo answers false
    Task<string?> AskAsync(CancellationToken cancellationToken);
    Task<CliChoice?> ChooseAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken);
    Task<IReadOnlyList<CliChoice>> ChooseManyAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken);
    void Pause();          // CliIo writes a blank line
    void Say(string something);
    void Say(string something, MessageLevel level);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}
```

`CliAppBuilder.Run` registers the I/O as a singleton from the app you chose,
unless a registry registered an `ICliIo` first: register your own to assert
on what a command printed, or to host KitCli somewhere with no console at
all. `CliIo` is the `Console`-backed default.

A `HeadlessCliApp` runs with `HeadlessCliIo` instead. It writes to the console like `CliIo`, but its
input has nothing attached: `CanAsk` says so ahead of time, and asking or
offering a choice anyway throws `NoInputException`, a programming error rather than an end
of input. `CanAsk` has a default body, so an I/O
of your own inherits an answer of true. A handler or factory that injects
`ICliIo` reads it to decline a prompt nobody would answer.

## Cancelling a blocked read

`Console.ReadLine` cannot be cancelled once it blocks. So `AskAsync` runs
it on a background thread, races it against the token, and returns `null`
if cancellation wins — abandoning the read where it stands rather than
waiting for a keypress that may never come.

**`null` also means end-of-input**, and `RespondToAsk` treats both the same
way. A piped stream running dry ends a run like a Ctrl+C does.

## Picking from a list

`ChooseAsync` and `ChooseManyAsync` show a Spectre selection prompt: arrow
keys to move, enter to pick, space to tick in the many-choice list. A
`CliChoice` is a `Label` to show and an optional `Value` handed back with the
pick. Spectre's prompt takes the cancellation token itself, so a Ctrl+C
returns `null` or an empty list without the background-thread race `AskAsync`
needs. An empty list of choices throws.

**A prompt redraws itself in place, so a line printed while one is showing
scrambles it.** Nothing in `CliIo` prevents that; print before asking or
after the answer. `CliIo` takes an optional `IAnsiConsole`, so a test can
drive the prompt with Spectre's `TestConsole`.

## Saying a message at a level

`Say(something, level)` writes only when `level` meets
`KitCliSettings.MinimumMessageLevel`, the rule leveled outcomes follow
([outcome writing](outcome-writing.md)). It is for code that has no outcome
list to return, such as progress lines from a pipeline inside a command.
`CliIo` reads the setting from its optional `KitCliSettings` argument and
falls back to the default, `Information`.

## What a Ctrl+C does

```
Console.CancelKeyPress
  → CliIo sets e.Cancel = true      (suppresses .NET's abrupt exit)
      → the Action registered via OnCancel
          → Workflow.InterruptCurrentRun()
```

`CliApp.SetUpEventHandlers` registers that action once and nothing else —
no `OnSessionEnd`, no `Environment.Exit`. The token itself belongs to
`ICliWorkflow`. Cancellation is cooperative, so **a handler that ignores
its token runs to completion.**

## See also

[cli-app-host.md](cli-app-host.md) · [outcome-writing.md](outcome-writing.md) ·
[../adr/0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md)
