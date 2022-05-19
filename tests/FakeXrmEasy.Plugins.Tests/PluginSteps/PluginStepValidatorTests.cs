using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
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
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registration_for_valid_combinations(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.AppNotification, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameContants.SearchTelemetry, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_upsert_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_primary_entity_name_for_upsert(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_create_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_create_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_update_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_update_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_retrieve_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Retrieve, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_retrieve_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_retrieve_multiple_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.RetrieveMultiple, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_retrieve_multiple_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_delete_message_as_valid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Delete, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_delete_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_any_other_message_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Assign, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_registrations_for_any_other_message_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData("*", "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("*", "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_valid_registrations_for_custom_actions_or_apis_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.True(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }

        [Theory]
        [InlineData("*", "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("*", "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_custom_actions_or_apis_by_default(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            Assert.False(_validator.IsValid(messageName, entityLogicalName, stage, mode));
        }
    }
}
