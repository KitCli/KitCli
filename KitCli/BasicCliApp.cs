using KitCli.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public class BasicCliApp : CliApp
{
    public BasicCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }
}