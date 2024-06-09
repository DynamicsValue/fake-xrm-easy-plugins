using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using System;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Plugins.PluginInstances;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    /// <summary>
    /// Unit tests to check the actual plugin is effectively registered in the In-Memory db
    /// </summary>
    public class PluginStepBasicRegistrationTests
    {
        private IXrmFakedContext _context;

        public PluginStepBasicRegistrationTests()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePipelineSimulation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
        }

        [Fact]
        public void Should_throw_exception_is_entity_type_code_was_used_without_proxy_types_assemblies()
        {
            Assert.Throws<RegisterEntityTypeCodePluginStepWithoutProxyTypesAssemblyException>(() =>
                _context.RegisterPluginStep<AccountNumberPlugin>(
                    new PluginStepDefinition() {
                        MessageName = "Create",
                        EntityTypeCode = DataverseEntities.Account.EntityTypeCode,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous
                    }));
        }
        
        [Fact]
        public void Should_throw_exception_is_entity_type_code_was_used_but_not_found_in_proxy_types_assemblies()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(DataverseEntities.Account)));
            
            Assert.Throws<RegisterInvalidEntityTypeCodePluginStepException>(() =>
                _context.RegisterPluginStep<AccountNumberPlugin>(
                    new PluginStepDefinition() {
                        MessageName = "Create",
                        EntityTypeCode = 9999999,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous
                    }));
        }
        
        [Fact]
        public void Should_throw_exception_if_both_entity_type_code_and_entity_logical_name_were_used_to_register_a_plugin_step()
        {
            Assert.Throws<RegisterStepWithEntityTypeCodeAndEntityLogicalNameException>(() =>
                _context.RegisterPluginStep<AccountNumberPlugin>(
                    new PluginStepDefinition() {
                        MessageName = "Create",
                        EntityTypeCode = DataverseEntities.Account.EntityTypeCode,
                        EntityLogicalName = DataverseEntities.Account.EntityLogicalName,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous
                    }));
        }
        
        [Theory]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_register_plugin_step_with_plugin_definition_and_plugin_type_signature(string messageName,
                                                        string entityLogicalName,
                                                        ProcessingStepStage stage,
                                                        ProcessingStepMode mode)
        {
            var pluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode
            });

            var processingStep = _context.CreateQuery<SdkMessageProcessingStep>().FirstOrDefault();
            Assert.NotNull(processingStep);
            Assert.Equal(pluginStepId, processingStep.Id);
            Assert.Equal((int)stage, processingStep.Stage.Value);
            Assert.Equal((int)mode, processingStep.Mode.Value);

            var sdkMessage = _context.CreateQuery<SdkMessage>()
                                    .Where(mes => mes.Id == processingStep.SdkMessageId.Id)
                                    .FirstOrDefault();
            Assert.NotNull(sdkMessage);
            Assert.Equal(messageName, sdkMessage.Name);

            var sdkMessageFilter = _context.CreateQuery<SdkMessageFilter>()
                                    .Where(messageFilter => messageFilter.Id == processingStep.SdkMessageFilterId.Id)
                                    .FirstOrDefault();
            Assert.NotNull(sdkMessageFilter);
            Assert.Equal(entityLogicalName, (string)sdkMessageFilter[SdkMessageFilterFieldNames.EntityLogicalName]);
        }
        
        [Theory]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, "secure", "unsecure")]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous,  "secure", "unsecure")]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous,  "secure", "unsecure")]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous,  "secure", "unsecure")]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous,  "secure", "unsecure")]
        public void Should_register_plugin_step_with_plugin_definition_and_secure_configurations(string messageName,
                                                        string entityLogicalName,
                                                        ProcessingStepStage stage,
                                                        ProcessingStepMode mode,
                                                        string secureConfig,
                                                        string unsecureConfig)
        {
            var pluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode,
                Configurations = new PluginStepConfigurations()
                {
                    SecureConfig = secureConfig,
                    UnsecureConfig = unsecureConfig
                }
            });

            var processingStep = _context.CreateQuery<SdkMessageProcessingStep>().FirstOrDefault();
            Assert.NotNull(processingStep);
            Assert.Equal(pluginStepId, processingStep.Id);
            Assert.Equal((int)stage, processingStep.Stage.Value);
            Assert.Equal((int)mode, processingStep.Mode.Value);
            Assert.Equal(unsecureConfig, processingStep.Configuration);

            var processingStepSecureConfig = _context.CreateQuery<SdkMessageProcessingStepSecureConfig>().FirstOrDefault();
            Assert.NotNull(processingStepSecureConfig);
            Assert.Equal(secureConfig, processingStepSecureConfig.SecureConfig);
        }
        
        [Theory]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameConstants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameConstants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameConstants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_register_plugin_step_with_plugin_definition_and_specific_plugin_instance(string messageName,
                                                        string entityLogicalName,
                                                        ProcessingStepStage stage,
                                                        ProcessingStepMode mode)
        {
            var pluginInstance = new AccountNumberPlugin();
            var pluginStepDefinition = new PluginStepDefinition()
            {
                Id = Guid.NewGuid(),
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode,
                PluginInstance = pluginInstance
            };
            var pluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(pluginStepDefinition);

            var processingStep = _context.CreateQuery<SdkMessageProcessingStep>().FirstOrDefault();
            Assert.NotNull(processingStep);
            Assert.Equal(pluginStepId, processingStep.Id);
            Assert.Equal((int)stage, processingStep.Stage.Value);
            Assert.Equal((int)mode, processingStep.Mode.Value);

            var pluginInstancesRepository = _context.GetProperty<IPluginInstancesRepository>();
            Assert.Equal(pluginInstance, pluginInstancesRepository.GetPluginInstance(pluginStepDefinition.Id));
        }

        [Fact]
        public void Should_register_plugin_with_plugin_id_if_one_was_passed_into_it()
        {
            var pluginStepId = Guid.NewGuid();
            var returnedPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                Id = pluginStepId,
                MessageName = "Create",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            Assert.Equal(pluginStepId, returnedPluginStepId);
            var processingStep = _context.CreateQuery<SdkMessageProcessingStep>().FirstOrDefault();
            Assert.NotNull(processingStep);
            Assert.Equal(pluginStepId, processingStep.Id);
        }

        [Fact]
        public void Should_return_error_if_a_plugin_with_the_same_id_was_previously_registered()
        {
            var pluginStepId = Guid.NewGuid();
            var returnedPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                Id = pluginStepId,
                MessageName = "Create",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            Assert.Throws<PluginStepDefinitionAlreadyRegisteredException>(() => _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                Id = pluginStepId,
                MessageName = "Create",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            }));
        }
    }
}
