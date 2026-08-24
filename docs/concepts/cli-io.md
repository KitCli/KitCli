# CLI I/O

`ICliIo` is the one seam through which a KitCli app reads input, writes
output, and learns about Ctrl+C. Nothing else in the framework touches
`Console`.

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
singleton. Replace it to test what a command printed, or to host KitCli
somewhere without a console.

## Cancelling a blocked read

`Console.ReadLine` cannot be cancelled once it blocks, so `AskAsync` races
it against the token on a background thread and returns `null` if
cancellation wins — abandoning the read where it stands rather than waiting
for a keypress that may never come.

**`null` also means end-of-input**, and `RespondToAsk` treats both as
`InvalidAsk`. A piped stream running dry ends a run like a Ctrl+C does.

## What a Ctrl+C does

```
Console.CancelKeyPress
  → CliIo sets e.Cancel = true      (suppresses .NET's abrupt exit)
      → the Action registered via OnCancel
          → Workflow.InterruptCurrentRun()
```

`CliApp.SetUpEventHandlers` registers that action once and nothing else —
no `OnSessionEnd`, no `Environment.Exit`. The token itself belongs to
`ICliWorkflow`. Cancellation is cooperative: a handler ignoring its token
runs to completion.

## See also

[cli-app-host.md](cli-app-host.md) · [outcome-writing.md](outcome-writing.md) ·
[0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md)
