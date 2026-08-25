using KitCli;
using KitCli.Playground.App;
using KitCli.Playground.Scenarios;

var aoo = new CliAppBuilder()
    .WithApp<TestCliApp>()
    .WithRegistry<PlaygroundScenarioRegistry>();

await aoo.Run();
