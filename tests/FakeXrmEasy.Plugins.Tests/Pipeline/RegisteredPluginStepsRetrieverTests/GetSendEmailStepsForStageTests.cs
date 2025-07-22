using System;
using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using System.Reflection;
using FakeXrmEasy.Pipeline.Exceptions;
using Microsoft.Crm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.RegisteredPluginStepsRetrieverTests
{
    public class GetSendEmailStepsForStageTests : FakeXrmEasyPipelineTestsBase
    {
        private const string SEND_EMAIL_REQUEST_NAME = "SendEmail";
        private readonly SendEmailRequest _request;
        private readonly Email _email;
        private readonly PipelineStageExecutionParameters _pipelineParameters;
        
        public GetSendEmailStepsForStageTests()
        {
            _email = new Email() { Id = Guid.NewGuid() };

            _request = new SendEmailRequest()
            {
                EmailId = _email.Id
            };

            _pipelineParameters = new PipelineStageExecutionParameters()
            {
                Request = _request
            };
        }

        [Fact]
        public void Should_return_empty_list_of_steps_if_none_were_registered()
        {
            _pipelineParameters.Stage = ProcessingStepStage.Preoperation;
            _pipelineParameters.Mode = ProcessingStepMode.Synchronous;
            
            _context.Initialize(_email);
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode(string messageName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = EntityLogicalNameConstants.Email,
                Stage = stage,
                Mode = mode
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            _context.Initialize(_email);
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(stage, pluginStep.Stage);
            Assert.Equal(mode, pluginStep.Mode);
            Assert.Equal(messageName, pluginStep.MessageName);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, pluginStep.AssemblyName);
            Assert.Equal(pluginType.FullName, pluginStep.PluginType);

            Assert.Empty(pluginStep.FilteringAttributes);
        }

        [Theory]
        [InlineData( ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData( ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData( ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData( ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_for_another_request_name(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Delete",
                Stage = stage,
                Mode = mode,
                EntityLogicalName = EntityLogicalNameConstants.Email
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            _context.Initialize(_email);
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_throw_exception_if_request_is_executed_but_the_email_does_not_exist(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                EntityLogicalName = EntityLogicalNameConstants.Email
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            Assert.Throws<PreEntityImageNotFoundException>(() => RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters));
        }
    }
}
