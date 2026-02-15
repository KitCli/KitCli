using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Parsers;

public interface IInstructionParser
{
    Instruction Parse(string terminalInput);
}