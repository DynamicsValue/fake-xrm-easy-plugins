using Crm;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using FakeXrmEasy.Tests;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class PluginStepAuditInPipelineTests
    {
        private IXrmFakedContext _context;
        private IOrganizationService _service;

        private readonly Account _account;
        private readonly Contact _contact;

        public PluginStepAuditInPipelineTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = new OptionSetValue(1),
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };

            _contact = new Contact()
            {
                Id = Guid.NewGuid()
            };
        }

        private IXrmFakedContext CreatePluginStepAuditEnabledContext()
        {
            return MiddlewareBuilder
                        .New()

                        // Add* -> Middleware configuration
                        .AddCrud()
                        .AddFakeMessageExecutors()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepAudit = true })

                        // Use* -> Defines pipeline sequence
                        .UsePipelineSimulation()
                        .UseMessages()
                        .UseCrud()
                        
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
        }

        private IXrmFakedContext CreatePluginStepAuditDisabledContext()
        {
            return MiddlewareBuilder
                        .New()

                        // Add* -> Middleware configuration
                        .AddCrud()
                        .AddFakeMessageExecutors()
                        .AddPipelineSimulation()

                        // Use* -> Defines pipeline sequence
                        .UsePipelineSimulation()
                        .UseMessages()
                        .UseCrud()
                        

                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
        }

        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation)]
        [InlineData(ProcessingStepStage.Preoperation)]
        [InlineData(ProcessingStepStage.Postoperation)]
        public void Should_capture_plugin_step_execution_for_several_stages_if_audit_is_enabled(ProcessingStepStage stage)
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = stage
            });

            var account = new Account() { Name = "Some name" };

            _service.Execute(new CreateRequest()
            {
                Target = account
            });

            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(stepsAudit);

            var auditedStep = stepsAudit[0];

            Assert.Equal("Create", auditedStep.MessageName);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(typeof(AccountNumberPlugin), auditedStep.PluginAssemblyType);
        }

        [Fact]
        public void Should_capture_multiple_plugin_step_executions_if_audit_is_enabled()
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation
            });

            _context.RegisterPluginStep<FollowupPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation
            });

            var account = new Account() { Name = "Some name" };

            _service.Execute(new CreateRequest()
            {
                Target = account
            });

            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Equal(2, stepsAudit.Count);

            Assert.Equal("Create", stepsAudit[0].MessageName);
            Assert.Equal(ProcessingStepStage.Preoperation, stepsAudit[0].Stage);
            Assert.Equal(typeof(AccountNumberPlugin), stepsAudit[0].PluginAssemblyType);
            Assert.Equal(1, stepsAudit[0].PluginContext.Depth);

            Assert.Equal("Create", stepsAudit[1].MessageName);
            Assert.Equal(ProcessingStepStage.Postoperation, stepsAudit[1].Stage);
            Assert.Equal(typeof(FollowupPlugin), stepsAudit[1].PluginAssemblyType);
            Assert.Equal(1, stepsAudit[1].PluginContext.Depth);
        }

        [Fact]
        public void Should_capture_the_correct_order_of_execution_when_two_plugins_are_registered_against_then_same_message_and_stage()
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<FollowupPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                EntityLogicalName = Account.EntityLogicalName,
                Rank = 2
            });

            _context.RegisterPluginStep<FollowupPlugin2>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                EntityLogicalName = Account.EntityLogicalName,
                Rank = 1
            });

            var account = new Account() { Name = "Some name" };

            _service.Execute(new CreateRequest()
            {
                Target = account
            });

            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Equal(2, stepsAudit.Count);

            Assert.Equal("Create", stepsAudit[0].MessageName);
            Assert.Equal(ProcessingStepStage.Postoperation, stepsAudit[0].Stage);
            Assert.Equal(typeof(FollowupPlugin2), stepsAudit[0].PluginAssemblyType);

            Assert.Equal("Create", stepsAudit[1].MessageName);
            Assert.Equal(ProcessingStepStage.Postoperation, stepsAudit[1].Stage);
            Assert.Equal(typeof(FollowupPlugin), stepsAudit[1].PluginAssemblyType);
        }

        [Fact]
        public void Should_throw_exception_if_querying_plugin_step_audit_without_it_being_enabled_in_pipeline()
        {
            _context = CreatePluginStepAuditDisabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation
            });

            var account = new Account() { Name = "Some name" };

            _service.Execute(new CreateRequest()
            {
                Target = account
            });

            Assert.Throws<TypeAccessException>(() => _context.GetProperty<IPluginStepAudit>());
            Assert.Throws<PluginStepAuditNotEnabledException>(() => _context.GetPluginStepAudit());
        }

        [Fact]
        public void Should_populate_output_parameters_in_plugin_step_audit()
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<FollowupPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                EntityLogicalName = Account.EntityLogicalName
            });

            // Act
            var target = new Account
            {
                Name = "Test Account"
            };

            var accountId = _service.Create(target);

            // Assert
            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(stepsAudit);

            Assert.Single(stepsAudit[0].OutputParameters);
            Assert.Equal(accountId, stepsAudit[0].OutputParameters["id"]);
        }

        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation)]
        [InlineData(ProcessingStepStage.Preoperation)]
        [InlineData(ProcessingStepStage.Postoperation)]
        public void Should_capture_plugin_step_execution_with_configurations_for_several_stages_if_audit_is_enabled(ProcessingStepStage stage)
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            var secureConfig = "FakeSecureConfig";
            var unsecureConfig = "FakeUnsecureConfig";
            
            _context.RegisterPluginStep<ConfigurationPluginPipeline>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = stage,
                Configurations = new PluginStepConfigurations()
                {
                    SecureConfig = secureConfig,
                    UnsecureConfig = unsecureConfig
                }
            });

            _service.Create(new Entity("account"));

            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(stepsAudit);

            var auditedStep = stepsAudit[0];

            Assert.Equal("Create", auditedStep.MessageName);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(typeof(ConfigurationPluginPipeline), auditedStep.PluginAssemblyType);
            Assert.Equal(secureConfig, auditedStep.PluginStepDefinition.Configurations.SecureConfig);
            Assert.Equal(unsecureConfig, auditedStep.PluginStepDefinition.Configurations.UnsecureConfig);

        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation)]
        [InlineData(ProcessingStepStage.Preoperation)]
        [InlineData(ProcessingStepStage.Postoperation)]
        public void Should_capture_plugin_step_execution_with_plugin_instance_for_several_stages_if_audit_is_enabled(ProcessingStepStage stage)
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            var pluginInstance = new CustomInstancePluginPipeline("My Injected Value");
            
            _context.RegisterPluginStep<ConfigurationPluginPipeline>(new PluginStepDefinition()
            {
                EntityLogicalName = Account.EntityLogicalName,
                MessageName = "Create",
                Stage = stage,
                PluginInstance = pluginInstance
            });

            _service.Create(new Entity("account"));

            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(stepsAudit);

            var auditedStep = stepsAudit[0];

            Assert.Equal("Create", auditedStep.MessageName);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(typeof(CustomInstancePluginPipeline), auditedStep.PluginAssemblyType);
            Assert.Equal(pluginInstance, auditedStep.PluginStepDefinition.PluginInstance);
        }
        
        /* Will work once DynamicsValue/fake-xrm-easy#31 is implemented 

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation)]
        [InlineData("Create", ProcessingStepStage.Preoperation)]
        [InlineData("Create", ProcessingStepStage.Postoperation)]
        public void Should_capture_plugin_step_execution_for_several_stages_and_generic_requests_if_audit_is_enabled(string requestName, ProcessingStepStage stage)
        {
            _context = CreatePluginStepAuditEnabledContext();
            _service = _context.GetOrganizationService();

            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = stage
            });

            var account = new Account() { Name = "Some name" };

            _service.Execute(new OrganizationRequest()
            {
                RequestName = requestName,
                Parameters = new ParameterCollection
                {
                    { "Target", account }
                }
            });

            var pluginStepAudit = _context.GetProperty<IPluginStepAudit>();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(stepsAudit);

            var auditedStep = stepsAudit[0];

            Assert.Equal(requestName, auditedStep.MessageName);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(typeof(AccountNumberPlugin), auditedStep.PluginAssemblyType);
        }

        */
    }
}
