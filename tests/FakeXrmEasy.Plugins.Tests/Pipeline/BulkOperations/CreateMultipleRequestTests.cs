using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.BulkOperations
{
    public class CreateMultipleRequestTests: FakeXrmEasyPipelineWidthAuditTestsBase
    {
        private readonly Account _account;
        private List<Entity> _entities;
        
        public CreateMultipleRequestTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = account_accountcategorycode.Standard,
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };
            
            _entities = new List<Entity>() { _account };
        }
        
        [Fact]
        public void Should_trigger_registered_bulk_preoperation_preoperation_step()
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = "CreateMultiple",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Preoperation
            });

            var response = _service.Execute(_entities.ToCreateMultipleRequest());
            Assert.IsType<CreateMultipleResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal("CreateMultiple", auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(ProcessingStepStage.Preoperation, auditedStep.Stage);
        }
    }
}