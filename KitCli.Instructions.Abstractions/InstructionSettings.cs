namespace KitCli.Instructions.Abstractions;

/// <summary>
/// Configurable settings that control how instructions are recognized when parsed.
/// </summary>
public class InstructionSettings
{
    /// <summary>
    /// The character that identifies the start of an instruction name. Defaults to '/'.
    /// </summary>
    public char Prefix { get; set; } = '/';

    /// <summary>
    /// The prefix that identifies the start of an instruction argument. Defaults to "--".
    /// </summary>
    public string ArgumentPrefix { get; set; } = "--";
}
