using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps.PluginStepValidatorTests
{
    public class PluginStepValidatorUpsertMultipleTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepValidatorUpsertMultipleTests()
        {
            _validator = new PluginStepValidator();
        }
        
        [Theory]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.AppNotification)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.BackgroundOperation)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.CardStateItem)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.ComponentVersionNrdDataSource)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.ElasticFileAttachment)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.EventExpanderBreadcrumb)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.FlowLog)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.FlowRun)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.MsDynTimelinePin)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.NlsqRegistration)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.None)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.PowerPagesLog)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.RecentlyUsed)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SearchTelemetry)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SearchResultsCache)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SharedWorkspaceAccessToken)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SharedWorkspaceNr)]
        public void Should_return_valid_registration_for_valid_upsert_multiple_combinations(string messageName, string entityLogicalName)
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
        public void Should_return_invalid_registration_for_upsert_multiple_and_custom_entities()
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.UpsertMultiple,
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
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.AppNotification, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.AppNotification, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SearchTelemetry, ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData(MessageNameConstants.UpsertMultiple, EntityLogicalNameConstants.SearchTelemetry, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_invalid_for_invalid_registrations_for_upsert_multiple_message(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
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
