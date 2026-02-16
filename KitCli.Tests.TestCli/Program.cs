using KitCli;
using KitCli.Tests.TestCli;
using KitCli.Tests.TestCli.Commands;

var aoo = new CliAppBuilder()
    .WithCli<TestCliApp>()
    .WithRegistry<CommandRegistry>();
    
await aoo.Run();