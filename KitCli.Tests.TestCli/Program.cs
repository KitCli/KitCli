using KitCli;

var aoo = new CliAppBuilder()
    .WithBasicCli()
    .WithRegistry<CommandRegistry>();
    
await aoo.Run();