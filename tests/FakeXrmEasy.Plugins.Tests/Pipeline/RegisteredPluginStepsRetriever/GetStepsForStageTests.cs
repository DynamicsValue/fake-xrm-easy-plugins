using System;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Plugins.PluginInstances;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmFakedContextPipelineExtensions
{
    public class GetStepsForStageTests : FakeXrmEasyPipelineTestsBase
    {
        private readonly CreateRequest _createRequest;
        private readonly Account _target;

        public GetStepsForStageTests()
        {
            _target = new Account() { };

            _createRequest = new CreateRequest()
            {
                Target = _target
            };
        }

        [Fact]
        public void Should_return_empty_list_of_steps_if_none_were_registered()
        {
            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, _createRequest);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode
            });

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
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
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
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
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_for_another_request_name(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Update",
                Stage = stage,
                Mode = mode
            });

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest).ToList();
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
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
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
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
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
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_with_filtering_attributes_if_request_does_not_contain_such_attribute(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = requestName,
                Stage = stage,
                Mode = mode,
                FilteringAttributes = new string[] { "name" }
            });

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous, "secure", "unsecure")]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
            Assert.Single(steps);
            
            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(secureConfig, pluginStep.Configurations.SecureConfig);
            Assert.Equal(unsecureConfig, pluginStep.Configurations.UnsecureConfig);
            Assert.Equal(pluginStepDefinition.Configurations.SecureConfigId, pluginStep.Configurations.SecureConfigId);
        }
        
        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
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

            var steps = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, stage, mode, _createRequest);
            Assert.Single(steps);
            
            var pluginStep = steps.FirstOrDefault();
            Assert.Equal(pluginInstance, pluginStep.PluginInstance);
        }

    }
}
