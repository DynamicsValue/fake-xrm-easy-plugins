using System;
using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    /// <summary>
    /// Tests to verify the execution of plugins with secure and unsecure configurations
    /// </summary>
    public class PipelinePluginSecureConfigurationsTests: FakeXrmEasyPipelineTestsBase
    {
        private readonly Account _account;
        private readonly Contact _contact;

        public PipelinePluginSecureConfigurationsTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = new OptionSetValue(1),
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };
        }
        
        [Fact]
        public void Should_trigger_registered_plugin_with_configurations()
        {
            var secureConfig = "Fake Secure Config";
            var unsecureConfig = "Fake Unsecure Config";
            
            _context.RegisterPluginStep<ConfigurationPluginPipeline>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                Configurations = new PluginStepConfigurations()
                {
                    SecureConfig = secureConfig,
                    UnsecureConfig = unsecureConfig
                }
            });

            _service.Create(new Account() { });

            var dummyResponseEntityRecord = _context.CreateQuery("dummyconfigurationresponse").FirstOrDefault();
            Assert.NotNull(dummyResponseEntityRecord);
            Assert.Equal(secureConfig, dummyResponseEntityRecord["secure"]);
            Assert.Equal(unsecureConfig, dummyResponseEntityRecord["unsecure"]);
        }
    }
}