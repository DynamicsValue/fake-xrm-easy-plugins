using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Pipeline.PipelineTypes;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Issues
{
    public class Issue0095: FakeXrmEasyPipelineTestsBase
    {
        [Fact]
        public void Should_not_raise_reflected_typed_not_found_exception_when_using_early_bound_and_registering_steps()
        {
            _context.Initialize(new Account() { Id = Guid.NewGuid()});
            
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = FakeApiPlugin.Message,
                Stage = ProcessingStepStage.Postoperation
            });
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
