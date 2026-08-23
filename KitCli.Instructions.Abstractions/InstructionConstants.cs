namespace KitCli.Instructions.Abstractions;

/// <summary>
/// Default token values used when parsing and formatting instructions.
/// </summary>
public static class InstructionConstants
{
    /// <summary>
    /// The default character used to separate words within a command name.
    /// </summary>
    public const char DefaultCommandNameSeparator = '-';

    /// <summary>
    /// The default prefix that identifies the start of an instruction name.
    /// </summary>
    public const string DefaultNamePrefix = "/";

    /// <summary>
    /// The default prefix that identifies the start of an instruction argument.
    /// </summary>
    public const string DefaultArgumentPrefix = "--";

    /// <summary>
    /// The default character used to separate tokens within an instruction.
    /// </summary>
    public const char DefaultSpaceCharacter = ' ';
}