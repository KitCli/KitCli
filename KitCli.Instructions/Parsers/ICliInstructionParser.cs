using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Parsers;

public interface ICliInstructionParser
{
    CliInstruction Parse(string terminalInput);
}