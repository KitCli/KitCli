using KitCli.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli.Tooling.Release;

public class ReleaseCliApp(ICliWorkflow workflow, ICliIo io) : ArgsCliApp(workflow, io);
