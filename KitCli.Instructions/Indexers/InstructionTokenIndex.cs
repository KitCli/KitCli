namespace KitCli.Instructions.Indexers;

public record InstructionTokenIndex
{
    public bool Found { get; init; }
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
}
