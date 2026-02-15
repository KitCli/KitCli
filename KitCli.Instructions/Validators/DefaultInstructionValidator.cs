using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;

namespace KitCli.Instructions.Validators;

// TODO: Write unit tests.
public class DefaultInstructionValidator : IInstructionValidator
{
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