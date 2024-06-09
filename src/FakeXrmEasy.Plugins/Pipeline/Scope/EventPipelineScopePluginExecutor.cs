using FakeXrmEasy.Plugins;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline.Scope
{
    internal static class EventPipelineScopePluginExecutor
    {
        /// <summary>
        /// Executes a specific instance of a plugin against the current scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith(EventPipelineScope scope, IPlugin instance)
        {
            var pluginContextProperties = scope.PluginContextProperties;
            var pluginContext = scope.PluginContext;
            return FakePluginExecutor.ExecutePluginWith(pluginContextProperties, pluginContext, instance);
        }
        
        /// <summary>
        /// Executes a plugin using the default constructor against the current scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith<T>(EventPipelineScope scope)
            where T : IPlugin, new()
        {
            var pluginContextProperties = scope.PluginContextProperties;
            var pluginContext = scope.PluginContext;
            return FakePluginExecutor.ExecutePluginWith<T>(pluginContextProperties, pluginContext);
        }

        /// <summary>
        /// Executes a plugin with configurations against the current scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="unsecureConfiguration"></param>
        /// <param name="secureConfiguration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IPlugin ExecutePluginWithConfigurations<T>(EventPipelineScope scope, string unsecureConfiguration, string secureConfiguration)
            where T : class, IPlugin
        {
            var pluginContextProperties = scope.PluginContextProperties;
            var pluginContext = scope.PluginContext;
            return FakePluginExecutor.ExecutePluginWithConfigurations<T>(pluginContextProperties, pluginContext, unsecureConfiguration, secureConfiguration);
        }
    }
}