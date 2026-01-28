using KitCli.Commands.Abstractions;

namespace KitCli.Commands;

public abstract record ListCliCommand : CliCommand
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    
    public static class ArgumentNames
    {
        public const string PageNumber = "pageNumber";
        public const string PageSize = "pageSize";
    }
    
    public static class ArtefactNames
    {
        public const string PageNumber = "pageNumber";
        public const string PageSize = "pageSize";
    }

    protected ListCliCommand(int? pageNumber = null, int? pageSize = null)
    {
        PageNumber = pageNumber ?? 1;
        PageSize = pageSize ?? 20;
    }
}