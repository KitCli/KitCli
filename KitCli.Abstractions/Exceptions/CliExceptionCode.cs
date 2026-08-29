namespace KitCli.Abstractions.Exceptions;

/// <summary>
/// Categorizes the kind of failure that produced a <see cref="CliException"/>.
/// </summary>
public enum CliExceptionCode
{
    /// <summary>
    /// The failure originated while processing an instruction.
    /// </summary>
    Instruction,

    /// <summary>
    /// The failure originated while processing a command.
    /// </summary>
    Command,

    /// <summary>
    /// The failure is application-specific and does not fall under one of the other predefined categories.
    /// </summary>
    Custom,

    /// <summary>
    /// No command generator was available to handle the request.
    /// </summary>
    NoCommandGenerator,

    /// <summary>
    /// No instruction was available to handle the request.
    /// </summary>
    NoInstruction,

    /// <summary>
    /// No reaction factory was available to handle the request.
    /// </summary>
    NoReactionFactory
}