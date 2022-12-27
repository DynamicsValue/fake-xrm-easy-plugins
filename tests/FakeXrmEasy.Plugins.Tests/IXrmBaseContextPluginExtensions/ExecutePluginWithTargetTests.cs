using Crm;
using FakeItEasy;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmBaseContextPluginExtensions
{
    public class ExecutePluginWithTargetTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_a_plugin_with_target_is_executed_the_inherent_plugin_was_also_executed_without_exceptions()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            //Execute our plugin against the selected target
            var fakedPlugin = _context.ExecutePluginWithTarget<RetrieveServicesPlugin>(target);

            //Assert that the plugin was executed
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .MustHaveHappened();
        }

        [Fact]
        public void When_a_plugin_with_target_is_executed_with_a_plugin_context_the_inherent_plugin_was_also_executed_without_exceptions()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };
            var pluginCtx = _context.GetDefaultPluginContext();

            //Execute our plugin against the selected target
            var fakedPlugin = _context.ExecutePluginWithTarget<RetrieveServicesPlugin>(pluginCtx, target);

            //Assert that the plugin was executed
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .MustHaveHappened();
        }

        [Fact]
        public void When_the_account_number_plugin_is_executed_it_adds_a_random_number_to_an_account_entity()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            //Execute our plugin against a target that doesn't contains the accountnumber attribute
            var fakedPlugin = _context.ExecutePluginWithTarget<AccountNumberPlugin>(target);

            //Assert that the target contains a new attribute
            Assert.True(target.Attributes.ContainsKey("accountnumber"));
        }

        [Fact]
        public void When_the_account_number_plugin_is_executed_for_an_account_that_already_has_a_number_exception_is_thrown()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };
            target["accountnumber"] = 69;

            //Execute our plugin against a target thatcontains the accountnumber attribute will throw exception
            Assert.Throws<InvalidPluginExecutionException>(() => _context.ExecutePluginWithTarget<AccountNumberPlugin>(target));
        }

        [Fact]
        public void When_the_followup_plugin_is_executed_for_an_account_it_should_create_a_new_task()
        {
            (_context as XrmFakedContext).ProxyTypesAssembly = Assembly.GetExecutingAssembly(); //Needed to be able to return early bound entities

            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            _context.ExecutePluginWithTarget<FollowupPlugin>(target);

            //The plugin creates a followup activity, check that that one exists
            var tasks = (from t in _context.CreateQuery<Task>()
                         select t).ToList();

            Assert.True(tasks.Count == 1);
            Assert.True(tasks[0]["subject"].Equals("Send e-mail to the new customer."));
        }

        [Fact]
        public void When_executing_a_plugin_which_inherits_from_iplugin_it_does_compile()
        {
            var fakedPlugin = _context.ExecutePluginWithTarget<MyPlugin>(new Entity());
        }
    }
}