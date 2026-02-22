using KitCli;
using KitCli.Playground;
using KitCli.Playground.Scenarios;

var aoo = new CliAppBuilder()
    .WithCli<TestCliApp>()
    .WithRegistry<PlaygroundScenarioRegistry>();
    
await aoo.Run();