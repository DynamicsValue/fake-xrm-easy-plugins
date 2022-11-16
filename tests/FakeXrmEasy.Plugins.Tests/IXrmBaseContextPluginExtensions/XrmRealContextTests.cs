using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmBaseContextPluginExtensions
{
    public class XrmRealContextTests : FakeXrmEasyTestsBase
    {
        private readonly XrmRealContext _realContext;

        public XrmRealContextTests(): base()
        {
            _realContext = new XrmRealContext(_service);
        }

        [Fact]
        public void Should_execute_plugin_with_real_context()
        {
            _context.EnableProxyTypes(Assembly.GetExecutingAssembly()); //Needed to be able to return early bound entities

            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", target);

            ParameterCollection outputParameters = new ParameterCollection();
            outputParameters.Add("id", guid1);

            _realContext.ExecutePluginWith<FollowupPlugin>(inputParameters, outputParameters, null, null);

            //The plugin creates a followup activity, check that that one exists
            var tasks = (from t in _context.CreateQuery<Crm.Task>()
                         select t).ToList();

            Assert.Single(tasks);
            Assert.Equal("Send e-mail to the new customer.", tasks[0].Subject);
            Assert.Equal(guid1, tasks[0].RegardingObjectId.Id);
        }
    }
}
