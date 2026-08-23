namespace KitCli.Instructions.Abstractions;

/// <summary>
/// Identifies the specific reason an <see cref="InstructionException"/> was thrown.
/// </summary>
public enum InstructionExceptionCode
{
    /// <summary>
    /// The instruction did not contain the required prefix.
    /// </summary>
    NoInstructionPrefix,

    /// <summary>
    /// The instruction did not contain a name.
    /// </summary>
    NoInstructionName,

    /// <summary>
    /// A required argument was not supplied.
    /// </summary>
    ArgumentIsRequired,
}