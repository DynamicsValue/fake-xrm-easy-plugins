using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace FakeXrmEasy.Tests
{
    /// <summary>
    /// Mock plugin to test pipeline simulation with specific plugin instances
    /// </summary>
    public class CustomInstancePluginPipeline : IPlugin
    {
        private readonly string _value;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public CustomInstancePluginPipeline()
        {
            _value = "";
        }
        
        /// <summary>
        /// Our custom constructor
        /// </summary>
        /// <param name="myInjectedValue">A sample parameter we use for DI purposes</param>
        public CustomInstancePluginPipeline(string myInjectedValue)
        {
            _value = myInjectedValue;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var target = (Entity)context.InputParameters["Target"];
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            service.Create(new Entity("dummyinstanceresponse")
            {
                ["value"] = _value
            });
        }
    }
}