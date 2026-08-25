using System.Reflection;
using KitCli.Abstractions;
using KitCli.Commands.Abstractions.Io;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KitCli;

/// <summary>
/// Fluent builder for assembling and running a KitCli app: picks the concrete <see cref="CliApp"/>
/// (and therefore whether it is interactive or headless), wires up optional configuration sources, registers
/// command registries, and resolves and runs the built app.
/// </summary>
public class CliAppBuilder
{
    private static readonly ServiceProviderOptions ServiceProviderOptions = new()
    {
        ValidateScopes = true,
        ValidateOnBuild = true
    };

    private readonly ServiceCollection _services = [];
    private ConfigurationBuilder? _configurationBuilder;
    private IConfigurationRoot? _configuration;

    /// <summary>
    /// Configures this app to run as a <see cref="BasicCliApp"/> — the default interactive app with no
    /// lifecycle hooks overridden.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithBasicApp()
    {
        _services.AddCli<BasicCliApp>();

        return this;
    }

    /// <summary>
    /// Configures this app to run as <typeparamref name="TCliApp"/> — use this to run a custom
    /// <see cref="CliApp"/> or <see cref="HeadlessCliApp"/> subclass instead of the basic app.
    /// </summary>
    /// <typeparam name="TCliApp">The concrete <see cref="CliApp"/> subclass to run.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithApp<TCliApp>() where TCliApp : CliApp
    {
        _services.AddCli<TCliApp>();

        return this;
    }

    /// <summary>
    /// Adds the calling assembly's user secrets as a configuration source, for reading settings via
    /// <see cref="WithSettings{TSettings}"/>.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithUserSecretSettings()
    {
        SetUpConfigurationBuilder();

        var callingAssembly = Assembly.GetCallingAssembly();
        
        _configurationBuilder!
            .AddUserSecrets(callingAssembly, optional: true, reloadOnChange: true);

        return this;
    }

    /// <summary>
    /// Adds a JSON file, resolved relative to the current directory, as a configuration source, for
    /// reading settings via <see cref="WithSettings{TSettings}"/>.
    /// </summary>
    /// <param name="fileName">The JSON file name to load; missing files are treated as optional.</param>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithJsonSettings(string fileName)
    {
        SetUpConfigurationBuilder();
        
        var currentDirectory = Directory.GetCurrentDirectory();
            
        _configurationBuilder!
            .SetBasePath(currentDirectory)
            .AddJsonFile(fileName, optional: true, reloadOnChange: true);

        return this;
    }
    
    /// <summary>
    /// Binds a configuration section, named after <typeparamref name="TSettings"/> with any trailing
    /// "Settings" removed, and registers it as an <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TSettings">The settings type to bind and register.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="Exception">Thrown if no configuration source has been added yet via a <c>With[..]Settings</c> call.</exception>
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
    
    /// <summary>
    /// Instantiates <typeparamref name="TRegistry"/> and calls its <see cref="ICliAppRegistry.Register"/>
    /// to register that registry's commands and services into this builder's service collection.
    /// </summary>
    /// <typeparam name="TRegistry">The parameterless command registry to instantiate and run.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithRegistry<TRegistry>() where TRegistry : ICliAppRegistry, new()
    {
        var registry = new TRegistry();
        
        registry.Register(_services);
        
        return this;
    }
    
    /// <summary>
    /// Resolves <typeparamref name="TSettings"/> via the same lookup <see cref="WithSettings{TSettings}"/>
    /// uses, instantiates <typeparamref name="TRegistry"/>, and calls its
    /// <see cref="IConfigurableCliAppRegistry{TSettings}.Register"/> with both.
    /// </summary>
    /// <typeparam name="TSettings">The settings type the registry needs to configure itself.</typeparam>
    /// <typeparam name="TRegistry">The settings-driven command registry to instantiate and run.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public CliAppBuilder WithRegistry<TSettings, TRegistry>()
        where TSettings : class
        where TRegistry : IConfigurableCliAppRegistry<TSettings>, new()
    {
        var settings = GetSettings<TSettings>();
        var registry = new TRegistry();
        
        registry.Register(settings!, _services);

        return this;
    }

    /// <summary>
    /// Builds the service provider, resolves the registered <see cref="CliApp"/>, and runs it through
    /// <see cref="CliApp.Run"/>, passing <paramref name="args"/> for an app that sources its ask from
    /// them.
    /// </summary>
    /// <param name="args">The process args to run a <see cref="HeadlessCliApp"/> with; ignored by an interactive app.</param>
    /// <returns>The running task for the resolved app's <see cref="CliApp.Run"/> call.</returns>
    /// <exception cref="AggregateException">
    /// Thrown at startup if any registered service can't be constructed, or if a singleton depends on a
    /// <c>Scoped</c> service — the provider is built with both validations on.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the resolved app is a <see cref="HeadlessCliApp"/> and no <paramref name="args"/> were
    /// provided — it has no way to ask for one.
    /// </exception>
    public Task Run(string[]? args = null)
    {
        EnsureInstructionSettingsRegistered();

        var serviceProvider = _services.BuildServiceProvider(ServiceProviderOptions);

        var cliApp = serviceProvider.GetRequiredService<CliApp>();

        var outcomeIoWriters = serviceProvider
            .GetServices<IOutcomeIoWriter>()
            .ToList();

        var argsProvided = args is { Length: > 0 };

        if (cliApp is HeadlessCliApp && !argsProvided)
        {
            var cliAppName = cliApp.GetType().Name;
            var noArgsMessage = $"{cliAppName} is a HeadlessCliApp and requires at least one argument to run — none were provided.";

            throw new ArgumentException(noArgsMessage);
        }

        return cliApp.Run(outcomeIoWriters, args);
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