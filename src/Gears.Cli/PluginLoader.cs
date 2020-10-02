using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gears.Formatters;
using Gears.Outputs;
using Gears.Services;
using Gears.Transformers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gears.Core.Plugins
{

    public class PluginLoader : IPluginLoader
    {
        private readonly ILogger<PluginLoader> logger;
        private readonly IServiceCollection services;

        public PluginLoader(ILogger<PluginLoader> logger, IServiceCollection services)
        {
            this.logger = logger;
            this.services = services;
        }

        public void ScanAndLoad(string[] pluginPaths)
        {
            if (pluginPaths is null)
            {
                throw new ArgumentNullException(nameof(pluginPaths));
            }

            var commands = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                var plugins = new List<Type>();

                plugins.AddRange(RegisterPlugins<IInput>(pluginAssembly));
                plugins.AddRange(RegisterPlugins<IOutput>(pluginAssembly));
                plugins.AddRange(RegisterPlugins<IFormatter>(pluginAssembly));
                plugins.AddRange(RegisterPlugins<ITransformer>(pluginAssembly));
                plugins.AddRange(RegisterPlugins<IBatchService>(pluginAssembly));

                if (plugins.Count == 0)
                {
                    logger.LogWarning("No plugins loaded from {Path}", pluginPath);
                    // string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                    // throw new ApplicationException(
                    //     $"Can't find any type which implements {typeof(TPlugin).Name} in {assembly} from {assembly.Location}.\n" +
                    //     $"Available types: {availableTypes}");
                }
                return plugins;

            }).ToList();

            foreach (Type command in commands)
            {
                logger.LogDebug("Loaded {PluginName} plugin", command.FullName);
                //Console.WriteLine($"{command.Name}\t - {command.Description}");
            }
        }

        protected Assembly LoadPlugin(string relativePath)
        {
            // Navigate up to the solution root
            string root = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            logger.LogDebug("Loading commands from: {PluginLocation}", pluginLocation);
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        protected IEnumerable<Type> RegisterPlugins<TPlugin>(Assembly assembly)
        {

            foreach (Type type in assembly.GetTypes())
            {
                logger.LogDebug("Checking type: {PluginName}, with: {Interfaces}", type.FullName,
                    String.Join(',', type.GetInterfaces().Select(i => i.FullName)));
                if (type.GetInterfaces().Any<Type>(t => t.FullName == typeof(TPlugin).FullName))
                {
                    logger.LogInformation("Found plugin: {PluginName}", type.FullName);

                    services.AddTransient(typeof(TPlugin), type);

                    yield return type;
                }
            }


        }


        protected IEnumerable<Type> CreateCommands(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                logger.LogDebug("Checking type: {PluginName}, with: {Interfaces}", type.FullName,
                    String.Join(',', type.GetInterfaces().Select(i => i.FullName)));
                if (type.GetInterfaces().Any<Type>(t => t.FullName == typeof(IOutput).FullName))
                {
                    logger.LogDebug("Found plugin: {PluginName}", type.FullName);

                    services.AddTransient(typeof(IOutput), type);

                    count++;
                    yield return type;
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements IOutput in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
