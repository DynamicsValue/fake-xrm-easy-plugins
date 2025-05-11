using Microsoft.Xrm.Sdk;
using System;
using PluginsForTesting;

namespace FakeXrmEasy.Tests.PluginsForTesting
{
    public class FollowupPlugin : IPlugin
    {
        /// <summary>
        /// A plug-in that creates a follow-up task activity when a new account is created.
        /// </summary>
        /// <remarks>Register this plug-in on the Create message, account entity,
        /// and asynchronous mode.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            FollowUpPluginCommon.Execute(serviceProvider);
        }
    }
}