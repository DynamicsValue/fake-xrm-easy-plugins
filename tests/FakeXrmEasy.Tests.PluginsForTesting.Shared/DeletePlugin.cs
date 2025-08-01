using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.Tests.PluginsForTesting
{
    public class DeletePlugin : IPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is EntityReference)
            {
                tracing.Trace("Contains target");

                // Obtain the target entity from the input parameters.
                EntityReference entityRef = (EntityReference)context.InputParameters["Target"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entityRef.LogicalName == "account")
                {
                    tracing.Trace("Is Account");
                }
            }
        }
    }
}