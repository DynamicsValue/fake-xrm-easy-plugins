using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps.PluginStepValidatorTests
{
    public class PluginStepValidatorUpdateTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepValidatorUpdateTests()
        {
            _validator = new PluginStepValidator();
        }

        [Theory]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_update_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
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
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_update_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
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
