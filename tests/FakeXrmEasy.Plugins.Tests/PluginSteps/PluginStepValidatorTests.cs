using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    public class PluginStepValidatorTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepValidatorTests()
        {
            _validator = new PluginStepValidator();
        }

        [Theory]
        [InlineData("upsert", "appnotification", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("upsert", "appnotification", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("upsert", "appnotification", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("upsert", "appnotification", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("upsert", "searchtelemetry", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("upsert", "searchtelemetry", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("upsert", "searchtelemetry", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("upsert", "searchtelemetry", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registration_for_valid_combinations(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData("upsert", "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        public void Should_return_invalid_registration_for_invalid_combinations(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }
    }
}
