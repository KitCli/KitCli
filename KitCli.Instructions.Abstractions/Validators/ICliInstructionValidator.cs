namespace KitCli.Instructions.Abstractions.Validators;

public interface ICliInstructionValidator
{
    bool IsValidInstruction(CliInstruction instruction);
}