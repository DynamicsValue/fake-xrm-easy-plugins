using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps.PluginStepValidatorTests
{
    public class PluginStepValidatorDefaultTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepValidatorDefaultTests()
        {
            _validator = new PluginStepValidator();
        }
        
        [Theory]
        [InlineData("*", "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_custom_actions_or_apis_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode
            };
            Assert.True(_validator.IsValid(pluginStepDefinition));
        }

        [Theory]
        [InlineData("*", "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("*", "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_custom_actions_or_apis_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode
            };
            Assert.False(_validator.IsValid(pluginStepDefinition));
        }
    }
}
