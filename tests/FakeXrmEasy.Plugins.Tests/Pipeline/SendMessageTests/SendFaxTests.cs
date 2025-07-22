
using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
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
    public class SendFaxTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        private readonly Fax _fax;
        private readonly SendFaxRequest _sendFaxRequest;
        
        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";
        
        public SendFaxTests()
        {
            _fax = new Fax()
            {
                Id = Guid.NewGuid(),
                Subject = "FXE Test"
            };

            _sendFaxRequest = new SendFaxRequest()
            {
                FaxId = _fax.Id
            };
        }
        
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        public void Should_trigger_send_fax_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = Fax.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(_fax);
            
            XAssert.ThrowsFaultCode(ErrorCodes.NotSupported, () => _service.Execute(_sendFaxRequest));
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Send, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
            Assert.Equal(_fax.Id, auditedStep.InputParameters["FaxId"]);
        }
    }
}