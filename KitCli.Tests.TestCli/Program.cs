using KitCli;

var aoo = new CliAppBuilder()
    .WithCli<TestCliApp>()
    .WithRegistry<CommandRegistry>();
    
await aoo.Run();