using System.Reflection;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// Dependency injection registration helpers for discovering <see cref="CliCommand"/> types, their factories,
/// and their MediatR handlers from an assembly.
/// </summary>
public static class CommandServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers every command factory (see <see cref="AddCommandFactories"/>) and MediatR command/handler
        /// found in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan for <see cref="CliCommand"/> implementations.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <exception cref="NullReferenceException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="assembly"/> contains no <see cref="CliCommand"/> implementations.</exception>
        public IServiceCollection AddCommandsFromAssembly(Assembly? assembly)
        {
            if (assembly == null)
            {
                throw new NullReferenceException("No Assembly Containing ICommand Implementation");
            }

            if (!assembly.AnyClassTypesImplementType(typeof(CliCommand)))
            {
                throw new ArgumentException($"No ICommand Implementations Found in Assembly '{assembly.FullName}'");
            }

            return services
                .AddCommandFactories(assembly)
                .AddReactionFactories(assembly)
                .AddMediatRCommandsAndHandlers(assembly);
        }

        private IServiceCollection AddMediatRCommandsAndHandlers(Assembly assembly)
            => services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        /// <summary>
        /// Registers a factory for every command type found in <paramref name="assembly"/>. Commands
        /// sharing a registration key — a primary command and its sub-commands, grouped by
        /// <see cref="GroupCommandsByRootWord"/> — are handled as one family: every hand-written factory
        /// for the family is registered under the family's root type, whichever specific member it
        /// actually builds, and every family member nothing was hand-written for gets
        /// <see cref="FallbackCliSubCommandFactory{TPrimaryCliCommand,TCreate}"/> instead.
        /// </summary>
        /// <param name="assembly">The assembly to scan for <see cref="CliCommand"/> and factory types.</param>
        /// <returns>The same service collection, for chaining.</returns>
        private IServiceCollection AddCommandFactories(Assembly assembly)
        {
            var commandImplementationTypes = assembly.WhereClassTypesImplementType(typeof(CliCommand)).ToList();
            var factoryImplementationTypes = assembly.WhereClassTypesImplementGenericType(typeof(CliCommandFactory<>));

            var families = GroupCommandsByRootWord(commandImplementationTypes);

            foreach (var (rootType, siblingTypes) in families)
            {
                var handWrittenFactories = factoryImplementationTypes
                    .Where(factory => factory.BaseType!.FirstGenericArgumentIs(rootType))
                    .ToList();

                foreach (var handWrittenFactory in handWrittenFactories)
                {
                    services.AddCommandFactory(rootType, handWrittenFactory);
                }

                var coveredTypes = GetCommandTypesNamedByFactories(handWrittenFactories, commandImplementationTypes);
                var familyHasOtherMembers = siblingTypes.Count > 0;
                var familyMembers = new[] { rootType }.Concat(siblingTypes);

                foreach (var member in familyMembers)
                {
                    if (coveredTypes.Contains(member))
                    {
                        continue;
                    }

                    if (member.GetConstructor(Type.EmptyTypes) is null)
                    {
                        continue;
                    }

                    var fallbackFactoryType = GetFallbackFactoryType(rootType, member, familyHasOtherMembers);

                    services.AddCommandFactory(rootType, fallbackFactoryType);
                }
            }

            return services;
        }

        /// <summary>
        /// Reads each factory's own class name to work out which command type it builds. A factory's
        /// declared type parameter only ever says which family it's keyed under, never which specific
        /// member it builds — but every hand-written factory in this codebase is already named after the
        /// exact thing it builds, so that's read back here instead.
        /// </summary>
        /// <param name="factories">The hand-written factories to read.</param>
        /// <param name="commandTypes">The known command types, to match an implied name against.</param>
        /// <returns>The command types a factory's class name identifies it as building.</returns>
        private static HashSet<Type> GetCommandTypesNamedByFactories(IEnumerable<Type> factories, IEnumerable<Type> commandTypes)
            => factories
                .Select(factory => factory.Name.EndsWith("Factory", StringComparison.Ordinal)
                    ? factory.Name[..^"Factory".Length]
                    : factory.Name)
                .Select(impliedCommandName => commandTypes.FirstOrDefault(candidate => candidate.Name == impliedCommandName))
                .Where(covered => covered != null)
                .Select(covered => covered!)
                .ToHashSet();

        /// <summary>
        /// Picks the automatic fallback for a command type with no factory of its own. A standalone
        /// command — sharing its key with nothing else — gets today's plain, unconditional fallback, since
        /// nothing else is registered under its key to conflict with. Any command sharing a key with
        /// another family member needs the fallback that only matches its own sub-command word instead; an
        /// unconditional fallback there would win every ask under that key, reported as the crash in #289.
        /// </summary>
        /// <param name="rootType">The family's root command type — what the fallback is registered under.</param>
        /// <param name="member">The specific family member the fallback must build.</param>
        /// <param name="familyHasOtherMembers">Whether <paramref name="member"/> shares its key with anything else.</param>
        /// <returns>The fallback factory type to register.</returns>
        private static Type GetFallbackFactoryType(Type rootType, Type member, bool familyHasOtherMembers)
            => member == rootType && !familyHasOtherMembers
                ? typeof(BasicCliCommandFactory<>).MakeGenericType(member)
                : typeof(FallbackCliSubCommandFactory<,>).MakeGenericType(rootType, member);

        /// <summary>
        /// Groups command types by the longest other command type's stripped name each one starts with —
        /// e.g. <c>HelloMateCliCommand</c> groups under <c>HelloCliCommand</c>. A command with no such
        /// match groups under itself.
        /// </summary>
        /// <param name="commandTypes">The command types to group.</param>
        /// <returns>Each root command type mapped to the other command types grouped under it.</returns>
        private static Dictionary<Type, List<Type>> GroupCommandsByRootWord(IEnumerable<Type> commandTypes)
        {
            var commandTypeList = commandTypes.ToList();

            var groups = new Dictionary<Type, List<Type>>();

            foreach (var commandType in commandTypeList)
            {
                var strippedName = CliCommand.StripCommandName(commandType.Name);

                var rootType = commandTypeList
                    .Where(candidate => candidate != commandType)
                    .Where(candidate => CliCommand.StripCommandName(candidate.Name).Length < strippedName.Length)
                    .Where(candidate => strippedName.StartsWith(CliCommand.StripCommandName(candidate.Name), StringComparison.Ordinal))
                    .OrderByDescending(candidate => CliCommand.StripCommandName(candidate.Name).Length)
                    .FirstOrDefault() ?? commandType;

                if (!groups.TryGetValue(rootType, out var members))
                {
                    members = [];
                    groups[rootType] = members;
                }

                if (rootType != commandType)
                {
                    members.Add(commandType);
                }
            }

            return groups;
        }

        private IServiceCollection AddReactionFactories(Assembly assembly)
        {
            var reactionImplementationTypes = assembly.WhereClassTypesImplementType(typeof(CliCommandReaction));
            var factoryImplementationTypes = assembly.WhereClassTypesImplementGenericType(typeof(CliCommandReactionFactory<>));

            foreach (var reactionType in reactionImplementationTypes)
            {
                var matchingFactories = factoryImplementationTypes
                    .Where(factory => factory.BaseType!.FirstGenericArgumentIs(reactionType))
                    .ToList();

                if (matchingFactories.Count > 1)
                {
                    throw new ArgumentException($"Multiple factories found for reaction type '{reactionType.Name}'");
                }

                if (matchingFactories.Count == 1)
                {
                    services.AddKeyedSingleton(typeof(ICliCommandReactionFactory), reactionType, matchingFactories.First());
                    continue;
                }

                var hasEmptyConstructor = reactionType.GetConstructor(Type.EmptyTypes) is not null;
                if (hasEmptyConstructor)
                {
                    var basicFactoryType = typeof(BasicCliCommandReactionFactory<>).MakeGenericType(reactionType);
                    services.AddKeyedSingleton(typeof(ICliCommandReactionFactory), reactionType, basicFactoryType);
                }
            }

            return services;
        }

        private void AddCommandFactory(Type commandImplementationType, Type factoryImplementationType)
        {
            var specificCommandName = CliCommand.StripCommandName(commandImplementationType.Name);
            
            var commandName = CliCommand.GetInstructionName(commandImplementationType);
            var shorthandCommandName = specificCommandName.ToLowerTitleCharacters();
        
            services
                .AddKeyedSingleton(
                    typeof(ICliCommandFactory),
                    commandName,
                    factoryImplementationType)
                .AddKeyedSingleton(
                    typeof(ICliCommandFactory),
                    shorthandCommandName,
                    factoryImplementationType);

            var commandAliases = commandImplementationType.GetCliCommandAliasNames();
            foreach (var commandAlias in commandAliases)
            {
                services
                    .AddKeyedSingleton(
                        typeof(ICliCommandFactory),
                        commandAlias,
                        factoryImplementationType);
            }
        }
    }
}