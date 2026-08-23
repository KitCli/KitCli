namespace KitCli.Instructions.Abstractions.Validators;

/// <summary>
/// Validates whether a parsed instruction is well-formed.
/// </summary>
public interface IInstructionValidator
{
    /// <summary>
    /// Determines whether the given instruction is valid.
    /// </summary>
    /// <param name="instruction">The instruction to validate.</param>
    /// <returns><see langword="true"/> if the instruction is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(Instruction instruction);
}