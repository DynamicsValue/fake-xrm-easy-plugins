using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.Tests.PluginsForTesting
{
    public class ExceptionLoverPlugin : IPlugin
    {
        public const string PluginExceptionMessage = "This is an amazing exception raised from a plugin!";
        public void Execute(IServiceProvider serviceProvider)
        {
            throw new InvalidPluginExecutionException(PluginExceptionMessage);
        }
    }
}