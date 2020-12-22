using Crm;
using FakeItEasy;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using FakeXrmEasy.Abstractions.Plugins;

namespace FakeXrmEasy.Plugins.Tests.IXrmFakedContextPluginExtensions
{
    public class ExecutePluginWithTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_the_followup_plugin_is_executed_for_an_account_after_create_it_should_create_a_new_task_with_a_regardingid()
        {
            (_context as XrmFakedContext).ProxyTypesAssembly = Assembly.GetExecutingAssembly(); //Needed to be able to return early bound entities

            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", target);

            ParameterCollection outputParameters = new ParameterCollection();
            outputParameters.Add("id", guid1);

            _context.ExecutePluginWith<FollowupPlugin>(inputParameters, outputParameters, null, null);

            //The plugin creates a followup activity, check that that one exists
            var tasks = (from t in _context.CreateQuery<Crm.Task>()
                         select t).ToList();

            Assert.True(tasks.Count == 1);
            Assert.True(tasks[0].Subject.Equals("Send e-mail to the new customer."));
            Assert.True(tasks[0].RegardingObjectId != null && tasks[0].RegardingObjectId.Id.Equals(guid1));
        }

        [Fact]
        public void When_A_Plugin_Is_Executed_With_And_Instance_That_one_is_executed()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            TestPropertiesPlugin plugin =
                new TestPropertiesPlugin()
                { Property = "Some test" };

            var inputParams = new ParameterCollection { new KeyValuePair<string, object>("Target", target) };

            //Execute our plugin against the selected target
            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.InputParameters = inputParams;

            _context.ExecutePluginWith(plugCtx, plugin);
            Assert.Equal("Property Updated", plugin.Property);
        }

        [Fact]
        public void When_initializing_the_context_with_Properties_Plugins_Can_Access_It()
        {
            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", new Entity());

            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.MessageName = "Create";
            plugCtx.InputParameters = inputParameters;

            var ex = Record.Exception(() => _context.ExecutePluginWith<TestContextPlugin>(plugCtx));
            Assert.Null(ex);
        }

        [Fact]
        public void When_OrganizationName_Set()
        {
            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.OutputParameters = new ParameterCollection();
            pluginCtx.OrganizationName = "TestOrgName";
            _context.ExecutePluginWith<TestContextOrgNamePlugin>(pluginCtx);

            Assert.True(pluginCtx.OutputParameters.ContainsKey("OrgName"));
            Assert.Equal("TestOrgName", pluginCtx.OutputParameters["OrgName"]);
        }

        [Fact]
        public void When_Passing_In_No_Properties_Plugins_Only_Get_Default_Values()
        {
            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", new Entity());

            var pluginContext = new XrmFakedPluginExecutionContext()
            {
                InputParameters = inputParameters,
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            //Parameters are defaulted now...
            var ex = Record.Exception(() => _context.ExecutePluginWith<TestContextPlugin>(pluginContext));
            Assert.Null(ex);

            pluginContext = new XrmFakedPluginExecutionContext()
            {
                InputParameters = inputParameters,
                MessageName = "Create",
                InitiatingUserId = Guid.NewGuid()
            };

            ex = Record.Exception(() => _context.ExecutePluginWith<TestContextPlugin>(pluginContext));
            Assert.Null(ex);

            pluginContext = new XrmFakedPluginExecutionContext()
            {
                InputParameters = inputParameters,
                MessageName = "Update",
                UserId = Guid.NewGuid()
            };

            ex = Record.Exception(() => _context.ExecutePluginWith<TestContextPlugin>(pluginContext));
            Assert.Null(ex);
        }

        [Fact]
        public void When_executing_a_plugin_primaryentityname_exists_in_the_context()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.PrimaryEntityName = "Account";
            pluginContext.MessageName = "Create";
            pluginContext.Stage = 20;

            var entity = new Entity();
            _context.ExecutePluginWith<PreAccountCreate>(pluginContext);

            Assert.True(true);
        }

        [Fact]
        public void When_getting_a_default_context_shared_variables_can_be_accessed_from_a_plugin()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.SharedVariables.Add("key", "somevalue");

            var ex = Record.Exception(() => _context.ExecutePluginWith<TestSharedVariablesPropertyPlugin>(pluginContext));
            Assert.Null(ex);
        }

        [Fact]
        public void When_executing_a_plugin_theres_no_need_to_pass_a_default_plugin_context_if_the_plugin_doesnt_need_it()
        {
            var entity = new Entity();
            _context.ExecutePluginWith<AccountNumberPlugin>();
            Assert.True(true);
        }

    }
}