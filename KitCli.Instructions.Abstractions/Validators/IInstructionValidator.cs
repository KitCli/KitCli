namespace KitCli.Instructions.Abstractions.Validators;

public interface IInstructionValidator
{
    bool IsValid(Instruction instruction);
}