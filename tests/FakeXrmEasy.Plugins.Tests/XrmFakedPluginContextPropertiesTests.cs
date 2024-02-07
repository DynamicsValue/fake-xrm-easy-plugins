

using System;
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests
{
    public class XrmFakedPluginContextPropertiesTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Example_about_retrieving_traces_written_by_plugin()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("account") { Id = guid1 };

            //Execute our plugin against a target that doesn't contains the accountnumber attribute
            _context.ExecutePluginWithTarget<AccountNumberPlugin>(target);

            //Get tracing service
            var fakeTracingService = _context.GetTracingService();
            var log = fakeTracingService.DumpTrace();

            //Assert that the target contains a new attribute
            Assert.Equal(log, $"Contains target{Environment.NewLine}Is Account{Environment.NewLine}");
        }

        [Fact]
        public void The_TracingService_Should_Be_Retrievable_Without_Calling_Execute_Before()
        {
            //Get tracing service
            var fakeTracingService = _context.GetPluginContextProperties().TracingService;

            Assert.NotNull(fakeTracingService);
        }

        [Fact]
        public void Retrieving_The_TracingService_Twice_Should_Return_The_Same_Instance()
        {
            //Get tracing service
            var fakeTracingService1 = _context.GetPluginContextProperties().TracingService;
            fakeTracingService1.Trace("foobar");

            var fakeTracingService2 = _context.GetPluginContextProperties().TracingService;

            Assert.NotNull(fakeTracingService1);
            Assert.NotNull(fakeTracingService2);

            Assert.Contains("foobar", fakeTracingService2.DumpTrace());
        }

        [Fact]
        public void Should_return_null_when_getting_an_invalid_service()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());

            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(_context.GetDefaultPluginContext());
            var service = serviceProvider.GetService(typeof(XrmFakedPluginContextPropertiesTests));
            
            Assert.Null(service);
        }

        [Fact]
        public void Should_return_fake_execution_context()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());

            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(_context.GetDefaultPluginContext());
            var executionContext = serviceProvider.GetService(typeof(IExecutionContext));

            Assert.NotNull(executionContext);
        }

        [Fact]
        public void Should_return_fake_plugin_execution_context()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());

            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(_context.GetDefaultPluginContext());
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext));

            Assert.NotNull(pluginExecutionContext);
        }
    }
}