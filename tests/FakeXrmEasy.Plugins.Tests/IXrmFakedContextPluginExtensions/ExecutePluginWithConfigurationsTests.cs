using FakeItEasy;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmFakedContextPluginExtensions
{
    public class ExecutePluginWithConfigurationsTests: FakeXrmEasyTestsBase
    {

        [Fact]
        public void When_A_Plugin_Is_Executed_Configurations_Can_Be_Used()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            var inputParams = new ParameterCollection { new KeyValuePair<string, object>("Target", target) };

            var unsecureConfiguration = "Unsecure Configuration";
            var secureConfiguration = "Secure Configuration";

            //Execute our plugin against the selected target
            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.InputParameters = inputParams;

            _context.ExecutePluginWithConfigurations<ConfigurationPlugin>(plugCtx, unsecureConfiguration, secureConfiguration);

            Assert.True(target.Contains("unsecure"));
            Assert.True(target.Contains("secure"));
            Assert.Equal((string)target["unsecure"], unsecureConfiguration);
            Assert.Equal((string)target["secure"], secureConfiguration);
        }

        [Fact]
        public void When_A_Plugin_Is_Executed_With_Configurations_But_Does_Not_Implement_Constructor_Throw_Exception()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            var inputParams = new ParameterCollection { new KeyValuePair<string, object>("Target", target) };

            var unsecureConfiguration = "Unsecure Configuration";
            var secureConfiguration = "Secure Configuration";

            //Execute our plugin against the selected target
            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.InputParameters = inputParams;

            Assert.Throws<ArgumentException>(() => _context.ExecutePluginWithConfigurations<FollowupPlugin>(plugCtx, unsecureConfiguration, secureConfiguration));
        }

 

        [Fact]
        public void When_A_Plugin_Is_Executed_With_Configurations_And_Instance_That_one_is_executed()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            var inputParams = new ParameterCollection { new KeyValuePair<string, object>("Target", target) };

            var unsecureConfiguration = "Unsecure Configuration";
            var secureConfiguration = "Secure Configuration";

            //Execute our plugin against the selected target
            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.InputParameters = inputParams;

            Assert.Throws<ArgumentException>(() => _context.ExecutePluginWithConfigurations<FollowupPlugin>(plugCtx, unsecureConfiguration, secureConfiguration));
        }

        [Fact]
        public void Should_invoke_plugin_with_configurations_when_inheriting_via_a_plugin_base_class()
        {
            var account = new Entity("account");
            account["name"] = "Hello World";
            account["address1_postcode"] = "1234";

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", account);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.Stage = 20;
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters = inputParameters;

            var ex = Record.Exception(() => _context.ExecutePluginWithConfigurations<AccountSetTerritories>(pluginCtx, null, null));
            Assert.Null(ex);
        }

        [Fact]
        public void Should_post_plugin_context_to_service_endpoint()
        {
            var endpointId = Guid.NewGuid();
            var fakedServiceEndpointNotificationService = _context.GetPluginContextProperties().ServiceEndpointNotificationService;

            A.CallTo(() => fakedServiceEndpointNotificationService.Execute(A<EntityReference>._, A<IExecutionContext>._))
                .Returns("response");

            var plugCtx = _context.GetDefaultPluginContext();

            var fakedPlugin =
                _context
                    .ExecutePluginWithConfigurations<ServiceEndpointNotificationPlugin>(plugCtx, endpointId.ToString(), null );


            A.CallTo(() => fakedServiceEndpointNotificationService.Execute(A<EntityReference>._, A<IExecutionContext>._))
                .MustHaveHappened();

        }

#if FAKE_XRM_EASY_9

        [Fact]
        public void Should_retrieve_entity_data_source()
        {
            _context.GetPluginContextProperties().EntityDataSourceRetriever = new Entity("abc_customdatasource")
            {
                ["abc_crmurl"] = "https://...",
                ["abc_username"] = "abcd",
                ["abc_password"] = "1234"
            };
            var pluginContext = _context.GetDefaultPluginContext();
            var entity = new Entity();
            var query = new QueryExpression();
            pluginContext.InputParameters["Query"] = query;

            _context.ExecutePluginWithConfigurations<RetrieveMultipleDataProvider>(pluginContext, null, null);

            var outputParameters = pluginContext.OutputParameters["BusinessEntityCollection"] as EntityCollection;
            Assert.Equal(2, outputParameters.Entities.Count);
            Assert.Equal("abc_dataprovider", outputParameters.EntityName);
        }
#endif

    }
}