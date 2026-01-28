using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;

namespace KitCli.Instructions.Validators;

// TODO: Write unit tests.
public class DefaultCliInstructionValidator : ICliInstructionValidator
{
    public bool IsValidInstruction(CliInstruction instruction)
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