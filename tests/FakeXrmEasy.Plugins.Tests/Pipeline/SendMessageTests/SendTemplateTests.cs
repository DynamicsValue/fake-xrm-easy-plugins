
/*
using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Crm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.SendMessageTests
{
    public class SendTemplateTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        private readonly Template _template;
        private readonly SendTemplateRequest _sendTemplateRequest;
        
        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";
        
        public SendTemplateTests()
        {
            _template = new Template()
            {
                Id = Guid.NewGuid(),
                Subject = "FXE Test"
            };

            _sendTemplateRequest = new SendTemplateRequest()
            {
                TemplateId = _template.Id
            };
    
            //_context.EnableProxyTypes(Assembly.GetAssembly(typeof(Email)));
        }
        
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_send_template_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = Template.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(new List<Entity>()
            {
                _template
            });
            
            var response = _service.Execute(_sendTemplateRequest);
            Assert.IsType<SendTemplateResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Send, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
            Assert.Equal(_template.Id, auditedStep.InputParameters["TemplateId"]);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_update_email_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Update,
                EntityLogicalName = Email.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(new List<Entity>()
            {
                _template
            });
            
            var response = _service.Execute(_sendTemplateRequest);
            Assert.IsType<SendTemplateResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Update, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
        }
        
       
    }
}
*/