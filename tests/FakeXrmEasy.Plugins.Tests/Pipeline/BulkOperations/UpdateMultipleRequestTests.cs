#if FAKE_XRM_EASY_9
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
    public class UpdateMultipleRequestTests: FakeXrmEasyPipelineWidthAuditTestsBase
    {
        private readonly Account _account;
        private List<Entity> _entities;

        private const string UpdateMultipleMessage = "UpdateMultiple";
        private const string UpdateMessage = "Update";
        
        public UpdateMultipleRequestTests()
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
        
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_registered_bulk_step(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.Initialize(_account);
            
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = UpdateMultipleMessage,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            var response = _service.Execute(_entities.ToUpdateMultipleRequest());
            Assert.IsType<UpdateMultipleResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(UpdateMultipleMessage, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
        }

        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_registered_bulk_step_and_then_single_step_if_both_are_registered(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = UpdateMultipleMessage,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = UpdateMessage,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });
            
            var response = _service.Execute(_entities.ToUpdateMultipleRequest());
            Assert.IsType<UpdateMultipleResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Equal(2, auditedSteps.Count);

            var bulkAuditedStep = auditedSteps[0];
            Assert.Equal(UpdateMultipleMessage, bulkAuditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), bulkAuditedStep.PluginAssemblyType);
            Assert.Equal(stage, bulkAuditedStep.Stage);
            Assert.Equal(mode, bulkAuditedStep.Mode);
            
            var singleAuditedStep = auditedSteps[1];
            Assert.Equal(UpdateMessage, singleAuditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), singleAuditedStep.PluginAssemblyType);
            Assert.Equal(stage, singleAuditedStep.Stage);
            Assert.Equal(mode, singleAuditedStep.Mode);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_registered_single_step_and_then_bulk_step_if_both_are_registered(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = UpdateMultipleMessage,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = UpdateMessage,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });
            
            var response = _service.Execute(new UpdateRequest() { Target = _account });
            Assert.IsType<UpdateResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Equal(2, auditedSteps.Count);

            var bulkAuditedStep = auditedSteps[0];
            Assert.Equal(UpdateMessage, bulkAuditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), bulkAuditedStep.PluginAssemblyType);
            Assert.Equal(stage, bulkAuditedStep.Stage);
            Assert.Equal(mode, bulkAuditedStep.Mode);
            
            var singleAuditedStep = auditedSteps[1];
            Assert.Equal(UpdateMultipleMessage, singleAuditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), singleAuditedStep.PluginAssemblyType);
            Assert.Equal(stage, singleAuditedStep.Stage);
            Assert.Equal(mode, singleAuditedStep.Mode);
        }
        
    }
}
#endif