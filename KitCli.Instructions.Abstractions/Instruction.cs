namespace KitCli.Instructions.Abstractions;

public record Instruction(
    string? Prefix,
    string? Name,
    string? SubInstructionName,
    List<AnonymousInstructionArgument> Arguments);