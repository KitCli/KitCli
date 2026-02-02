using KitCli;
using KitCli.Instructions.Abstractions;
using KitCli.Tests.TestCli;

var aoo = new CliAppBuilder()
    .WithCli<TestCliApp>()
    .WithRegistry<CommandRegistry>();
    
await aoo.Run();