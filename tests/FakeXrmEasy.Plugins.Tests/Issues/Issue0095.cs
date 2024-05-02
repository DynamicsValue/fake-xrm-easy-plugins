using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Pipeline.PipelineTypes;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using System;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Issues
{
    public class Issue0095: FakeXrmEasyPipelineTestsBase
    {
        [Fact]
        public void Should_not_raise_reflected_typed_not_found_exception_when_using_early_bound_and_registering_steps()
        {
            _context.Initialize(new DataverseEntities.Account() { Id = Guid.NewGuid()});
            
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = FakeApiPlugin.Message,
                Stage = ProcessingStepStage.Postoperation
            });

            var sdkMessages = _context.CreateQuery<SdkMessage>().ToList();
            Assert.Single(sdkMessages);
        }
        
        [Fact]
        public void Should_have_all_necessary_entities_for_pipeline_simulation_without_raising_exception()
        {
            _context.Initialize(new DataverseEntities.SdkMessage() { Id = Guid.NewGuid()});
            var ex = Record.Exception(() => PluginStepRegistrationManager.AddPipelineTypes(_context));
            Assert.Null(ex);
        }

        [Fact]
        public void Should_throw_warning_exception_if_only_a_subset_of_entities_were_generated()
        {
            _context.Initialize(new DataverseEntitiesPartial.Account() { Id = Guid.NewGuid() });

            Assert.Throws<MissingPipelineTypesException>(() => _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = FakeApiPlugin.Message,
                Stage = ProcessingStepStage.Postoperation
            }));
        }
    }
}
