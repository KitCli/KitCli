namespace KitCli.Instructions.Indexers;

public record CliInstructionTokenIndex
{
    public bool Found { get; init; }
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
}
