using System;
using System.Linq;
using FakeXrmEasy.Abstractions.Plugins;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins
{
    internal static class FakePluginExecutor
    {
        /// <summary>
        /// Executes a plugin with the specified properties and plugin context. It will use the parameterless constructor.
        /// </summary>
        /// <param name="pluginContextProperties"></param>
        /// <param name="plugCtx"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IPlugin ExecutePluginWith<T>(IXrmFakedPluginContextProperties pluginContextProperties,
            XrmFakedPluginExecutionContext plugCtx) 
            where T : IPlugin, new()
        {
            var fakedServiceProvider = FakeServiceProviderFactory.New(pluginContextProperties, plugCtx);
            var fakePlugin = FakePluginFactory.New<T>(fakedServiceProvider);
            fakePlugin.Execute(fakedServiceProvider);
            return fakePlugin;
        }
        
        /// <summary>
        /// Executes a plugin with the specified properties and plugin context. It will use the instance provided as opposed to the parameterless constructor.
        /// </summary>
        /// <param name="pluginContextProperties"></param>
        /// <param name="plugCtx"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        internal static IPlugin ExecutePluginWith(IXrmFakedPluginContextProperties pluginContextProperties,
            XrmFakedPluginExecutionContext plugCtx,
            IPlugin instance)
        {
            var fakedServiceProvider = FakeServiceProviderFactory.New(pluginContextProperties, plugCtx);
            var fakePlugin = FakePluginFactory.New(fakedServiceProvider, instance);
            fakePlugin.Execute(fakedServiceProvider);
            return fakePlugin;
        }
        
        /// <summary>
        /// Executes a plugin with the specified properties and plugin context and unsecure and secure configurations
        /// </summary>
        /// <param name="pluginContextProperties"></param>
        /// <param name="plugCtx"></param>
        /// <param name="unsecureConfiguration"></param>
        /// <param name="secureConfiguration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static IPlugin ExecutePluginWithConfigurations<T>(
            IXrmFakedPluginContextProperties pluginContextProperties,
            XrmFakedPluginExecutionContext plugCtx, string unsecureConfiguration, string secureConfiguration)
            where T : class, IPlugin
        {
            var pluginType = typeof(T);
            var constructors = pluginType.GetConstructors().ToList();

            if (!constructors.Any(c => c.GetParameters().Length == 2 && c.GetParameters().All(param => param.ParameterType == typeof(string))))
            {
                throw new ArgumentException("The plugin you are trying to execute does not specify a constructor for passing in two configuration strings.");
            }

            var pluginInstance = (T)Activator.CreateInstance(typeof(T), unsecureConfiguration, secureConfiguration);
            
            var fakedServiceProvider = FakeServiceProviderFactory.New(pluginContextProperties, plugCtx);
            var fakePlugin = FakePluginFactory.New(fakedServiceProvider, pluginInstance);
            fakePlugin.Execute(fakedServiceProvider);
            return fakePlugin;
        }
    }
}