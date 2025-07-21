using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace FakeXrmEasy.Tests
{
    /// <summary>
    /// Mock plugin to test pipeline simulation with secure and unsecure configurations
    /// </summary>
    public class ConfigurationPluginPipeline : IPlugin
    {
        private readonly string _unsecureConfiguration;
        private readonly string _secureConfiguration;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public ConfigurationPluginPipeline() : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Constructor with configurations
        /// </summary>
        /// <param name="unsecureConfiguration">The unsecure configuration</param>
        /// <param name="secureConfiguration">The secure configuration</param>
        public ConfigurationPluginPipeline(string unsecureConfiguration, string secureConfiguration)
        {
            _unsecureConfiguration = unsecureConfiguration;
            _secureConfiguration = secureConfiguration;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var target = (Entity)context.InputParameters["Target"];
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            
            target["unsecure"] = _unsecureConfiguration;
            target["secure"] = _secureConfiguration;

            service.Create(new Entity("dummyconfigurationresponse")
            {
                ["unsecure"] = _unsecureConfiguration,
                ["secure"] = _secureConfiguration
            });
        }
    }
}