using KitCli.Abstractions.Io;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// Records every line written to it, in order. A pause records an empty line, matching the blank
/// line the real console IO writes.
/// </summary>
public class TestCliIo : ICliIo
{
    public List<string> Lines { get; } = [];

    public Task<string?> AskAsync(CancellationToken cancellationToken)
        => Task.FromResult<string?>(null);

    public void Pause()
        => Lines.Add(string.Empty);

    public void Say(string something)
        => Lines.Add(something);

    public void SetTitle(string title)
    {
    }

    public void OnCancel(Action cancel)
    {
    }
}
