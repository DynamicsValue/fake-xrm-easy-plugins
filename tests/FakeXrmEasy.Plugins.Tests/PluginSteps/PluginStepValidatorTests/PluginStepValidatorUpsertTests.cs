using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps.PluginStepValidatorTests
{
    public class PluginStepValidatorUpsertTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepValidatorUpsertTests()
        {
            _validator = new PluginStepValidator();
        }

        [Theory]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.AppNotification)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.BackgroundOperation)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.CardStateItem)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.ComponentVersionNrdDataSource)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.ElasticFileAttachment)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.EventExpanderBreadcrumb)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.FlowLog)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.FlowRun)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.MsDynTimelinePin)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.NlsqRegistration)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.None)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.PowerPagesLog)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.RecentlyUsed)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SearchTelemetry)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SearchResultsCache)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SharedWorkspaceAccessToken)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SharedWorkspaceNr)]
        public void Should_return_valid_registration_for_upsert_valid_combinations(string messageName, string entityLogicalName)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
            };

            pluginStepDefinition.Stage = ProcessingStepStage.Prevalidation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.True(_validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Preoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.True(_validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Postoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.True(_validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Postoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Asynchronous;
            Assert.True(_validator.IsValid(pluginStepDefinition));
        }

        [Fact]
        public void Should_return_invalid_registration_for_upsert_and_custom_entities()
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Upsert,
                EntityLogicalName = Account.EntityLogicalName,
            };

            pluginStepDefinition.Stage = ProcessingStepStage.Prevalidation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Preoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Postoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Synchronous;
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(pluginStepDefinition));
            
            pluginStepDefinition.Stage = ProcessingStepStage.Postoperation;
            pluginStepDefinition.Mode = ProcessingStepMode.Asynchronous;
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(pluginStepDefinition));
        }
        
        [Theory]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.AppNotification, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.AppNotification, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SearchTelemetry, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.Upsert, EntityLogicalNameConstants.SearchTelemetry, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_upsert_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
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

        [Theory]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Upsert, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_primary_entity_name_for_upsert(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode
            };
            Assert.Throws<InvalidPrimaryEntityNameException>(() => _validator.IsValid(pluginStepDefinition));
        }
    }
}
