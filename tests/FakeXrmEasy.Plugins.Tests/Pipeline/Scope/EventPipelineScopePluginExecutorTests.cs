using System;
using System.Collections.Generic;
using FakeItEasy;
using FakeXrmEasy.Pipeline.Scope;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.Scope
{
    public class EventPipelineScopePluginExecutorTests: FakeXrmEasyTestsBase
    {
        private readonly IPipelineOrganizationService _pipelineOrganizationService;
        private readonly EventPipelineScope _scope;

        public EventPipelineScopePluginExecutorTests()
        {
            _scope = new EventPipelineScope();
            _pipelineOrganizationService = PipelineOrganizationServiceFactory.New(_service, _scope);

            _scope.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _pipelineOrganizationService,
                _context.GetTracingService());
        }

        [Fact]
        public void Should_execute_plugin_with_specific_instance()
        {
            _scope.PluginContext = _context.GetDefaultPluginContext();
            var plugin = EventPipelineScopePluginExecutor.ExecutePluginWith(_scope, new TracerPlugin());
            A.CallTo(() => plugin.Execute(A<IServiceProvider>._)).MustHaveHappened();
        }
        
        [Fact]
        public void Should_execute_plugin_with_default_constructor()
        {
            _scope.PluginContext = _context.GetDefaultPluginContext();
            var plugin = EventPipelineScopePluginExecutor.ExecutePluginWith<TracerPlugin>(_scope);
            A.CallTo(() => plugin.Execute(A<IServiceProvider>._)).MustHaveHappened();
        }
        
        [Fact]
        public void Should_execute_plugin_with_configurations()
        {
            var guid1 = Guid.NewGuid();
            var target = new Entity("contact") { Id = guid1 };

            var inputParams = new ParameterCollection { new KeyValuePair<string, object>("Target", target) };

            var unsecureConfiguration = "Unsecure Configuration";
            var secureConfiguration = "Secure Configuration";

            //Execute our plugin against the selected target
            var plugCtx = _context.GetDefaultPluginContext();
            plugCtx.InputParameters = inputParams;
            
            _scope.PluginContext = plugCtx;
            var plugin = EventPipelineScopePluginExecutor.ExecutePluginWithConfigurations<ConfigurationPlugin>(_scope, unsecureConfiguration, secureConfiguration);
            A.CallTo(() => plugin.Execute(A<IServiceProvider>._)).MustHaveHappened();
            
            Assert.True(target.Contains("unsecure"));
            Assert.True(target.Contains("secure"));
            Assert.Equal((string)target["unsecure"], unsecureConfiguration);
            Assert.Equal((string)target["secure"], secureConfiguration);
        }
    }
}