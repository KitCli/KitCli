using System.Reflection;
using KitCli.Abstractions;
using KitCli.Commands.Abstractions.Io;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KitCli;

public class CliAppBuilder
{
    private readonly ServiceCollection _services = [];
    private ConfigurationBuilder? _configurationBuilder;
    private IConfigurationRoot? _configuration;
    
    public CliAppBuilder WithBasicCli()
    {
        _services.AddCli<BasicCliApp>();
        
        return this;
    }
    
    public CliAppBuilder WithCli<TCliApp>() where TCliApp : CliApp
    {
        _services.AddCli<TCliApp>();
        
        return this;
    }

    public CliAppBuilder WithUserSecretSettings()
    {
        SetUpConfigurationBuilder();

        var callingAssembly = Assembly.GetCallingAssembly();
        
        _configurationBuilder!
            .AddUserSecrets(callingAssembly, optional: true, reloadOnChange: true);

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
    
    public CliAppBuilder WithRegistry<TRegistry>() where TRegistry : ICliAppBuilderRegistry, new()
    {
        var registry = new TRegistry();
        
        registry.Register(_services);
        
        return this;
    }
    
    public CliAppBuilder WithRegistry<TSettings, TRegistry>()
        where TSettings : class
        where TRegistry : ICliAppBuilderConfigurableRegistry<TSettings>, new()
    {
        var settings = GetSettings<TSettings>();
        var registry = new TRegistry();
        
        registry.Register(settings!, _services);

        return this;
    }

    public async Task Run()
    {
        EnsureInstructionSettingsRegistered();
        
        var serviceProvider = _services.BuildServiceProvider();
        
        var cliApp = serviceProvider.GetRequiredService<CliApp>();
        
        var outcomeIoWriters = serviceProvider
            .GetServices<ICliCommandOutcomeIoWriter>();
        
        await cliApp.Run(outcomeIoWriters.ToList());
    }
    
    private void SetUpConfigurationBuilder()
    {
        if (_configurationBuilder == null)
        {
            _services.AddOptions();
            
            _configurationBuilder = new ConfigurationBuilder();
        }
    }
    
    private TSettings? GetSettings<TSettings>() where TSettings : class
    {
        if (_configurationBuilder == null && typeof(TSettings) == typeof(InstructionSettings))
        {
            return new InstructionSettings() as TSettings;
        }
        
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
        
        var sectionExists = section.Exists();
        
        if (!sectionExists && typeof(TSettings) == typeof(InstructionSettings))
        {
            return new InstructionSettings() as TSettings;
        }

        if (!sectionExists)
        {
            throw new Exception($"No configuration section found for {configurationName}");
        }

        return section.Get<TSettings>();
    }
    
    private void EnsureInstructionSettingsRegistered()
    {
        var anyInstructionSettingsRegistered = _services
            .Any(sd => sd.ServiceType == typeof(InstructionSettings));

        if (anyInstructionSettingsRegistered)
        {
            return;
        }
        
        var instructionSettings = GetSettings<InstructionSettings>();
        var options = new OptionsWrapper<InstructionSettings>(instructionSettings!);
        
        _services.AddSingleton<IOptions<InstructionSettings>>(options);
    }
}