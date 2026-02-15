namespace KitCli.Instructions.Abstractions;

public record Instruction(
    string? Prefix,
    string? Name,
    string? SubInstructionName,
    List<AnonymousInstructionArgument> Arguments)
{
    public static Instruction Empty = new(string.Empty, string.Empty, string.Empty, []);
}