using KitCli;
using KitCli.Instructions.Abstractions;
using KitCli.Tests.TestCli;

var aoo = new CliAppBuilder()
    .WithCli<TestCliApp>()
    .WithUserSecretSettings<TestCliApp>()
    .WithRegistry<CommandRegistry>()
    .WithRegistry<InstructionSettings, ConfiguredCommandRegistry>();
    
await aoo.Run();