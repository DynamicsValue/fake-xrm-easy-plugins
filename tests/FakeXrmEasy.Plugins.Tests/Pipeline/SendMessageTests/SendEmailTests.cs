
using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Reflection;
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
    public class SendEmailTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        private readonly Email _email;
        private List<Entity> _entities;

        private readonly SendEmailRequest _sendEmailRequest;
        
        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";
        
        public SendEmailTests()
        {
            _email = new Email()
            {
                Id = Guid.NewGuid(),
                Subject = "FXE Test"
            };

            _sendEmailRequest = new SendEmailRequest()
            {
                EmailId = _email.Id
            };
    
            //_context.EnableProxyTypes(Assembly.GetAssembly(typeof(Email)));
        }
        
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_send_email_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = Email.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(_email);
            
            var response = _service.Execute(_sendEmailRequest);
            Assert.IsType<SendEmailResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Send, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
            Assert.Equal(_email.Id, auditedStep.InputParameters["EmailId"]);
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

            _context.Initialize(_email);
            
            var response = _service.Execute(_sendEmailRequest);
            Assert.IsType<SendEmailResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Update, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_pass_preimage_when_there_is_a_registered_preimage(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, ProcessingStepImageType.PreImage);

            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = Email.EntityLogicalName,
                Stage = stage,
                Mode = mode,
                ImagesDefinitions = new List<PluginImageDefinition>()
                {
                    preImageDefinition
                }
            });

            _context.Initialize(new List<Entity>()
            {
                _email
            });
            
            //Act
            var response = _service.Execute(_sendEmailRequest);
            Assert.IsType<SendEmailResponse>(response);

            //Assert
            var allEmails = _context.CreateQuery<Email>().ToList();

            var sentEmail = allEmails.Where(a => a.Id == _email.Id);
            Assert.NotNull(sentEmail);

            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);
            var auditedStep = auditedSteps[0];

            var preImage = auditedStep.PluginContext.PreEntityImages[registeredPreImageName];
            Assert.NotNull(preImage);
            Assert.Equal(_email.Id, preImage.Id);
            Assert.Equal(Email.EntityLogicalName, preImage.LogicalName);
        }
    }
}