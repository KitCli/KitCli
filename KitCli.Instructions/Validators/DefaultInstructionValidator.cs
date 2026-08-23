using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;

namespace KitCli.Instructions.Validators;

// TODO: Write unit tests.
/// <summary>
/// Default <see cref="IInstructionValidator"/> implementation that requires an instruction to have both
/// a prefix and a name.
/// </summary>
public class DefaultInstructionValidator : IInstructionValidator
{
    /// <inheritdoc/>
    public bool IsValid(Instruction instruction)
    {
        if (instruction.Prefix is null)
        {
            return false;
        }

        if (instruction.Name is null)
        {
            return false;
        }

        return true;
    }
}