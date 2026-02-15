namespace KitCli.Instructions.Extraction;

public record InstructionTokenExtraction(
    string? PrefixToken,
    string? NameToken,
    string? SubNameToken,
    Dictionary<string, string?> ArgumentTokens);