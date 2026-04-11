

using System;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
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
        
        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_fake_plugin_execution_context2_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext2)) as IPluginExecutionContext2;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext2;
            Assert.Equal(expectedPlugCtx.UserAzureActiveDirectoryObjectId, pluginExecutionContext.UserAzureActiveDirectoryObjectId);
            Assert.Equal(expectedPlugCtx.InitiatingUserAzureActiveDirectoryObjectId, pluginExecutionContext.InitiatingUserAzureActiveDirectoryObjectId);
        }
        
        [Fact]
        public void Should_return_fake_plugin_execution_context3_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext3)) as IPluginExecutionContext3;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext3;
            Assert.Equal(expectedPlugCtx.AuthenticatedUserId, pluginExecutionContext.AuthenticatedUserId);
        }
        
        [Fact]
        public void Should_return_fake_plugin_execution_context4_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext4)) as IPluginExecutionContext4;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext4;
            Assert.Equal(expectedPlugCtx.PreEntityImagesCollection, pluginExecutionContext.PreEntityImagesCollection);
            Assert.Equal(expectedPlugCtx.PostEntityImagesCollection, pluginExecutionContext.PostEntityImagesCollection);
        }
        
        [Fact]
        public void Should_return_fake_plugin_execution_context5_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext5)) as IPluginExecutionContext5;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext5;
            Assert.Equal(expectedPlugCtx.InitiatingUserAgent, pluginExecutionContext.InitiatingUserAgent);
        }
        
        [Fact]
        public void Should_return_fake_plugin_execution_context6_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext6)) as IPluginExecutionContext6;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext6;
            Assert.Equal(expectedPlugCtx.EnvironmentId, pluginExecutionContext.EnvironmentId);
            Assert.Equal(expectedPlugCtx.TenantId, pluginExecutionContext.TenantId);
        }
        
        [Fact]
        public void Should_return_fake_plugin_execution_context7_with_defaults()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var plugCtx = _context.GetDefaultPluginContext();
            var serviceProvider = _context.PluginContextProperties.GetServiceProvider(plugCtx);
            var pluginExecutionContext = serviceProvider.GetService(typeof(IPluginExecutionContext7)) as IPluginExecutionContext7;
            Assert.NotNull(pluginExecutionContext);
            
            var expectedPlugCtx = plugCtx as XrmFakedPluginExecutionContext7;
            Assert.Equal(expectedPlugCtx.IsApplicationUser, pluginExecutionContext.IsApplicationUser);
        }
        #endif
        
    }
}