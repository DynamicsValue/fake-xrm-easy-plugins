using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Audit
{
    public class PluginStepAuditTests
    {
        private readonly PluginStepAudit _pluginStepAudit;

        public PluginStepAuditTests()
        {
            _pluginStepAudit = new PluginStepAudit();
        }

        [Fact]
        public void Should_return_a_query_with_empty_collection_if_no_steps_were_executed()
        {
            var pluginSteps = _pluginStepAudit.CreateQuery().ToList();
            Assert.Empty(pluginSteps);
        }

        [Fact]
        public void Should_return_a_plugin_step_audit_that_was_added()
        {
            var pluginStepAuditDetails = new PluginStepAuditDetails()
            {
                MessageName = "Create",
                PluginAssemblyType = typeof(AccountNumberPlugin),
                PluginStepId = Guid.NewGuid(),
                Stage = ProcessingStepStage.Preoperation,
                TargetEntity = new Entity(Contact.EntityLogicalName, Guid.NewGuid()),
                TargetEntityReference = new EntityReference(Contact.EntityLogicalName, Guid.NewGuid())
            };

            _pluginStepAudit.Add(pluginStepAuditDetails);

            var pluginSteps = _pluginStepAudit.CreateQuery().ToList();
            Assert.Single(pluginSteps);

            var first = pluginSteps.First();
            Assert.Equal(pluginStepAuditDetails.MessageName, first.MessageName);
            Assert.Equal(pluginStepAuditDetails.PluginAssemblyType, first.PluginAssemblyType);
            Assert.Equal(pluginStepAuditDetails.PluginStepId, first.PluginStepId);
            Assert.Equal(pluginStepAuditDetails.Stage, first.Stage);
            Assert.Equal(pluginStepAuditDetails.TargetEntity, first.TargetEntity);
            Assert.Equal(pluginStepAuditDetails.TargetEntityReference, first.TargetEntityReference);
            Assert.NotEqual(DateTime.MinValue, first.ExecutedOn); 
        }
    }
}
