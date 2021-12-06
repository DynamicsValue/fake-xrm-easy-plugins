using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.Plugins.Tests.PluginsForTesting
{
    public class ContextTestsPlugin : IPlugin
    {
        public IPluginExecutionContext PopulatedPluginContext { get; set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            PopulatedPluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        }
    }
}
