using System;
using System.Collections.Generic;
using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.RegisteredPluginStepsRetrieverTests
{
    public class GetBulkOperationsStepsForStageTests : FakeXrmEasyPipelineTestsBase
    {
        private readonly CreateMultipleRequest _createMultipleRequest;
        private readonly Account _target;
        private const string createMultipleRequestName = "CreateMultiple";

        private readonly PipelineStageExecutionParameters _pipelineParameters;
        
        public GetBulkOperationsStepsForStageTests()
        {
            _target = new Account() { };

            _createMultipleRequest = new CreateMultipleRequest()
            {
                Targets = new EntityCollection(new List<Entity>() { _target })
            };

            _pipelineParameters = new PipelineStageExecutionParameters()
            {
                Request = _createMultipleRequest
            };
        }

        [Fact]
        public void Should_return_empty_list_of_steps_if_none_were_registered()
        {
            _pipelineParameters.Stage = ProcessingStepStage.Preoperation;
            _pipelineParameters.Mode = ProcessingStepMode.Synchronous;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(stage, pluginStep.Stage);
            Assert.Equal(mode, pluginStep.Mode);
            Assert.Equal(requestName, pluginStep.MessageName);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, pluginStep.AssemblyName);
            Assert.Equal(pluginType.FullName, pluginStep.PluginType);

            Assert.Empty(pluginStep.FilteringAttributes);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_and_entity_type_code_if_present(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(DataverseEntities.Account)));
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                EntityTypeCode = Account.EntityTypeCode
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(stage, pluginStep.Stage);
            Assert.Equal(mode, pluginStep.Mode);
            Assert.Equal(requestName, pluginStep.MessageName);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, pluginStep.AssemblyName);
            Assert.Equal(pluginType.FullName, pluginStep.PluginType);
            Assert.Equal(Account.EntityTypeCode, pluginStep.EntityTypeCode);

            Assert.Empty(pluginStep.FilteringAttributes);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_for_another_request_name(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Update",
                Stage = stage,
                Mode = mode
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_multiple_registered_plugin_steps_for_exact_request_name_stage_and_mode_ordered_by_rank(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<FollowupPlugin2>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                Rank = 2
            });

            _context.RegisterPluginStep<FollowupPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                Rank = 1
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters).ToList();
            Assert.Equal(2, steps.Count);

            var firstPluginStep = steps[0];
            Assert.Equal(stage, firstPluginStep.Stage);
            Assert.Equal(mode, firstPluginStep.Mode);
            Assert.Equal(requestName, firstPluginStep.MessageName);

            var firstPluginType = typeof(FollowupPlugin);
            Assert.Equal(firstPluginType.Assembly.GetName().Name, firstPluginStep.AssemblyName);
            Assert.Equal(firstPluginType.FullName, firstPluginStep.PluginType);

            var secondPluginStep = steps[1];
            Assert.Equal(stage, secondPluginStep.Stage);
            Assert.Equal(mode, secondPluginStep.Mode);
            Assert.Equal(requestName, secondPluginStep.MessageName);

            var secondPluginType = typeof(FollowupPlugin2);
            Assert.Equal(secondPluginType.Assembly.GetName().Name, secondPluginStep.AssemblyName);
            Assert.Equal(secondPluginType.FullName, secondPluginStep.PluginType);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_with_filtering_attributes(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                FilteringAttributes = new string[] { "name" }
            });

            _target.Name = "Some name";

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(stage, pluginStep.Stage);
            Assert.Equal(mode, pluginStep.Mode);
            Assert.Equal(requestName, pluginStep.MessageName);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, pluginStep.AssemblyName);
            Assert.Equal(pluginType.FullName, pluginStep.PluginType);

            Assert.NotEmpty(pluginStep.FilteringAttributes);
            Assert.Equal("name", pluginStep.FilteringAttributes.First());
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_with_multiple_filtering_attributes(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                FilteringAttributes = new string[] { "name", "accountcategorycode" }
            });

            _target.AccountCategoryCode = new OptionSetValue(0);

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(stage, pluginStep.Stage);
            Assert.Equal(mode, pluginStep.Mode);
            Assert.Equal(requestName, pluginStep.MessageName);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, pluginStep.AssemblyName);
            Assert.Equal(pluginType.FullName, pluginStep.PluginType);

            var attributes = pluginStep.FilteringAttributes.ToList();
            Assert.NotEmpty(attributes);
            Assert.Equal("name", attributes[0]);
            Assert.Equal("accountcategorycode", attributes[1]);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_with_filtering_attributes_if_request_does_not_contain_such_attribute(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                FilteringAttributes = new string[] { "name" }
            });

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous, "secure", "unsecure")]
        public void Should_return_registered_plugin_step_with_secure_and_unsecure_configurations(
            string requestName, 
            ProcessingStepStage stage,
            ProcessingStepMode mode,
            string secureConfig,
            string unsecureConfig)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                Configurations = new PluginStepConfigurations()
                {
                    SecureConfigId = Guid.NewGuid(),
                    SecureConfig = secureConfig,
                    UnsecureConfig = unsecureConfig
                }
            };
            
            _context.RegisterPluginStep<AccountNumberPlugin>(pluginStepDefinition);

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);
            
            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(secureConfig, pluginStep.Configurations.SecureConfig);
            Assert.Equal(unsecureConfig, pluginStep.Configurations.UnsecureConfig);
            Assert.Equal(pluginStepDefinition.Configurations.SecureConfigId, pluginStep.Configurations.SecureConfigId);
        }
        
        [Theory]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(createMultipleRequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_with_specific_plugin_instance(
            string requestName, 
            ProcessingStepStage stage,
            ProcessingStepMode mode)
        {
            var pluginInstance = new AccountNumberPlugin();
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                PluginInstance = pluginInstance
            };
            
            _context.RegisterPluginStep<AccountNumberPlugin>(pluginStepDefinition);

            _pipelineParameters.Stage = stage;
            _pipelineParameters.Mode = mode;
            
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, _pipelineParameters);
            Assert.Single(steps);
            
            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(pluginInstance, pluginStep.PluginInstance);
        }

    }
}
