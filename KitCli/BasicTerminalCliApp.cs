using KitCli.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public class BasicTerminalCliApp : TerminalCliApp
{
    public BasicTerminalCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }
}