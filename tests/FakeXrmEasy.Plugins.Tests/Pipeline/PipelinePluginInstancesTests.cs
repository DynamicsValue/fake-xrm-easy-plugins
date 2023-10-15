using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    /// <summary>
    /// Tests to verify the execution of specific plugin instances in pipeline simulation
    /// </summary>
    public class PipelinePluginInstancesTests: FakeXrmEasyPipelineTestsBase
    {
        [Fact]
        public void Should_trigger_registered_plugin_with_plugin_instance()
        {
            var myPluginInstance = new CustomInstancePluginPipeline("My Injected Value");
            
            _context.RegisterPluginStep<ConfigurationPluginPipeline>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                PluginInstance = myPluginInstance
            });

            _service.Create(new Account() { });

            var dummyResponseEntityRecord = _context.CreateQuery("dummyinstanceresponse").FirstOrDefault();
            Assert.NotNull(dummyResponseEntityRecord);
            Assert.Equal("My Injected Value", dummyResponseEntityRecord["value"]);
        }
    }
}