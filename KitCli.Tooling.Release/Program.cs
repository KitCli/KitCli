using KitCli;
using KitCli.Tooling.Release;

var app = new CliAppBuilder()
    .WithApp<ReleaseCliApp>();

await app.Run(args);
