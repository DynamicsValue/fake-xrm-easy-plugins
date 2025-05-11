#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;

namespace FakeXrmEasy.Tests.PluginsForTesting
{
    public class FollowupPlugin4 : IPlugin
    {
        /// <summary>
        /// A plug-in that creates a follow-up task activity when a new account is created.
        /// </summary>
        /// <remarks>Register this plug-in on the Create message, account entity,
        /// and asynchronous mode.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext4 context = (IPluginExecutionContext4)
                serviceProvider.GetService(typeof(IPluginExecutionContext4));
            
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Targets") &&
                context.InputParameters["Targets"] is EntityCollection)
            {

                

                
            }
        }
    }
}
#endif