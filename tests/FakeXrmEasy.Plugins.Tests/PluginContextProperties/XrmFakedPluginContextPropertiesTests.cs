

using System;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginContextProperties
{
    public class TracingServiceTests
    {
        [Fact]
        public void Example_about_retrieving_traces_written_by_plugin()
        {
            var fakedContext = new XrmFakedContext();

            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            //Execute our plugin against a target that doesn't contains the accountnumber attribute
            var fakedPlugin = fakedContext.ExecutePluginWithTarget<AccountNumberPlugin>(target);

            //Get tracing service
            var fakeTracingService = fakedContext.GetFakeTracingService();
            var log = fakeTracingService.DumpTrace();

            //Assert that the target contains a new attribute
            Assert.Equal(log, $"Contains target{Environment.NewLine}Is Account{Environment.NewLine}");
        }

        [Fact]
        public void The_TracingService_Should_Be_Retrievable_Without_Calling_Execute_Before()
        {
            var fakedContext = new XrmFakedContext();

            //Get tracing service
            var fakeTracingService = fakedContext.GetPluginContextProperties().TracingService;

            Assert.NotNull(fakeTracingService);
        }

        [Fact]
        public void Retrieving_The_TracingService_Twice_Should_Return_The_Same_Instance()
        {
            var fakedContext = new XrmFakedContext();

            //Get tracing service
            var fakeTracingService1 = fakedContext.GetPluginContextProperties().TracingService;
            fakeTracingService1.Trace("foobar");

            var fakeTracingService2 = fakedContext.GetPluginContextProperties().TracingService;

            Assert.NotNull(fakeTracingService1);
            Assert.NotNull(fakeTracingService2);

            Assert.Contains("foobar", fakeTracingService2.DumpTrace());
        }
    }
}