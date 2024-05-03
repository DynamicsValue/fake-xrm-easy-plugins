using System;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Issues
{
    public class Issue00144: FakeXrmEasyPipelineWidthAuditTestsBase
    {
        private readonly Entity _account;

        public Issue00144()
        {
            _account = new Entity(Account.EntityLogicalName) { Id = Guid.NewGuid() };
        }
        
        [Fact]
        public void Should_trigger_delete_request_when_using_late_bound()
        {
            _context.RegisterPluginStep<DeletePlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Delete",
                Stage = ProcessingStepStage.Postoperation
            });
            
            _context.Initialize(_account);
            
            _service.Delete(Account.EntityLogicalName, _account.Id);

            var auditedSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps.First();
            Assert.Equal("Delete", auditedStep.MessageName);
            Assert.Equal(typeof(DeletePlugin), auditedStep.PluginAssemblyType);
        }
    }
}