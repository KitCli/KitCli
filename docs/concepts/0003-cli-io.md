# 0003. CLI I/O

A framework that calls `Console` directly cannot be tested and cannot be
hosted anywhere else. So KitCli calls it in exactly one class. `ICliIo` is
the seam: everything a KitCli app reads, writes, or learns about Ctrl+C
goes through it, and nothing else in the framework touches `Console`.

```csharp
public interface ICliIo
{
    Task<string?> AskAsync(CancellationToken cancellationToken);
    void Pause();          // CliIo writes a blank line
    void Say(string something);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}
```

`AddCliAbstractions` registers `CliIo`, the `Console`-backed default, as a
singleton. Replace it to assert on what a command printed, or to host
KitCli somewhere with no console at all.

## Cancelling a blocked read

`Console.ReadLine` cannot be cancelled once it blocks. So `AskAsync` runs
it on a background thread, races it against the token, and returns `null`
if cancellation wins — abandoning the read where it stands rather than
waiting for a keypress that may never come.

**`null` also means end-of-input**, and `RespondToAsk` treats both the same
way. A piped stream running dry ends a run like a Ctrl+C does.

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

[0002-cli-app-host.md](0002-cli-app-host.md) · [0004-outcome-writing.md](0004-outcome-writing.md) ·
[../adr/0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md)
