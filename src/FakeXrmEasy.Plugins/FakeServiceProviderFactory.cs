using System;
using FakeXrmEasy.Abstractions.Plugins;

namespace FakeXrmEasy.Plugins
{
    internal static class FakeServiceProviderFactory
    {
        public static IServiceProvider New(IXrmFakedPluginContextProperties pluginContextProperties,
            XrmFakedPluginExecutionContext plugCtx)
        {
            return pluginContextProperties.GetServiceProvider(plugCtx);
        }
    }
}