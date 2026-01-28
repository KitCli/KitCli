using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli;

public class CliAppBuilder
{
    private readonly ServiceCollection _services = [];
    private ConfigurationBuilder? _configurationBuilder;
    private IConfigurationRoot? _configuration;
    
    public CliAppBuilder WithCli<TCliApp>() where TCliApp : CliApp
    {
        _services.AddCli<TCliApp>();
        
        return this;
    }

    private void SetUpConfigurationBuilder()
    {
        if (_configurationBuilder == null)
        {
            _services.AddOptions();
            
            _configurationBuilder = new ConfigurationBuilder();
        }
    }

    // TODO: Verify i need the generic. Can I pass in calling assembly instead?
    public CliAppBuilder WithUserSecretSettings<TCliApp>() where TCliApp : CliApp
    {
        SetUpConfigurationBuilder();

        _configurationBuilder!
            .AddUserSecrets<TCliApp>(optional: true, reloadOnChange: true);

        return this;
    }

    public CliAppBuilder WithJsonSettings(string fileName)
    {
        SetUpConfigurationBuilder();
        
        var currentDirectory = Directory.GetCurrentDirectory();
            
        _configurationBuilder!
            .SetBasePath(currentDirectory)
            .AddJsonFile(fileName, optional: true, reloadOnChange: true);

        return this;
    }
    
    public CliAppBuilder WithSettings<TSettings>() where TSettings : class
    {
        if (_configurationBuilder == null)
        {
            throw new Exception("You must call With[..]Settings before calling WithSettings.");
        }
        
        if (_configuration == null)
        {
            _configuration = _configurationBuilder.Build();
        }
        
        var configurationName = typeof(TSettings)
            .Name
            .Replace("Settings", string.Empty);
        
        var section = _configuration.GetSection(configurationName);
        
        _services.Configure<TSettings>(section);

        return this;
    }

    public CliAppBuilder WithServices(Func<IServiceCollection, IServiceCollection> configureServices)
    {
        configureServices(_services);
        
        return this;
    }

    public CliAppBuilder WithConfiguredServices<TSettings>(Func<TSettings, IServiceCollection, IServiceCollection> configureServices)
    {
        if (_configurationBuilder == null)
        {
            throw new Exception("You must call With[..]Settings before calling WithSettings.");
        }
        
        if (_configuration == null)
        {
            _configuration = _configurationBuilder.Build();
        }
        
        var configurationName = typeof(TSettings)
            .Name
            .Replace("Settings", string.Empty);

        var section = _configuration.GetSection(configurationName);
        
        if (!section.Exists())
        {
            throw new Exception($"No configuration section found for {configurationName}");
        }

        var settings = section.Get<TSettings>();

        configureServices(settings!, _services);

        return this;
    }

    public async Task Run()
    {
        var serviceProvider = _services.BuildServiceProvider();
        
        var cliApp = serviceProvider.GetRequiredService<CliApp>();
        
        await cliApp.Run();
    }
}