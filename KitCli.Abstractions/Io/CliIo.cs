using Spectre.Console;

namespace KitCli.Abstractions.Io;

/// <summary>
/// Default <see cref="ICliIo"/> implementation backed by the standard <see cref="Console"/>.
/// </summary>
public class CliIo(IAnsiConsole? console = null) : ICliIo
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<CliChoice?> ChooseAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken)
    {
        var prompt = new SelectionPrompt<CliChoice>()
            .Title(Markup.Escape(title))
            .UseConverter(ShowChoiceAsPlainText)
            .AddChoices(choices);

        try
        {
            return await prompt.ShowAsync(_console, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CliChoice>> ChooseManyAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken)
    {
        var prompt = new MultiSelectionPrompt<CliChoice>()
            .Title(Markup.Escape(title))
            .Required(false)
            .UseConverter(ShowChoiceAsPlainText)
            .AddChoices(choices);

        try
        {
            return await prompt.ShowAsync(_console, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public void Pause()
        => Console.WriteLine();

    /// <inheritdoc/>
    public void Say(string something)
        => Console.WriteLine(something);

    /// <inheritdoc/>
    public void SetTitle(string title)
        => Console.Title = title;

    /// <inheritdoc/>
    public void OnCancel(Action cancel)
        => Console.CancelKeyPress += (_, e) =>
        {
            SuppressDefaultAbruptTermination(e);
            cancel();
        };

    private static string ShowChoiceAsPlainText(CliChoice choice)
        => Markup.Escape(choice.Label);

    private static void SuppressDefaultAbruptTermination(ConsoleCancelEventArgs e)
        => e.Cancel = true;
}