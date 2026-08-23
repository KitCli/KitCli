using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Parsers;

/// <summary>
/// Parses raw terminal input into a structured <see cref="Instruction"/>.
/// </summary>
public interface IInstructionParser
{
    /// <summary>
    /// Parses terminal input into a structured instruction.
    /// </summary>
    /// <param name="terminalInput">The raw terminal input to parse.</param>
    /// <returns>The parsed instruction.</returns>
    Instruction Parse(string terminalInput);
}