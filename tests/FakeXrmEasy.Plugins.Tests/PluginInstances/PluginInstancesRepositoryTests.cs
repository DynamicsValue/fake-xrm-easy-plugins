using System;
using FakeXrmEasy.Plugins.PluginInstances;
using FakeXrmEasy.Tests.PluginsForTesting;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginInstances
{
    public class PluginInstancesRepositoryTests
    {
        private readonly PluginInstancesRepository _repository;

        public PluginInstancesRepositoryTests()
        {
            _repository = new PluginInstancesRepository();
        }

        [Fact]
        public void Should_return_null_if_no_plugin_instances_exist()
        {
            Assert.Null(_repository.GetPluginInstance(Guid.NewGuid()));
        }
        
        [Fact]
        public void Should_return_specific_plugin_instance_previously_stored()
        {
            var pluginInstance = new AccountNumberPlugin();
            var stepId = Guid.NewGuid();
            _repository.SetPluginInstance(stepId, pluginInstance);
            Assert.Equal(pluginInstance, _repository.GetPluginInstance(stepId));
        }
        
        [Fact]
        public void Should_update_plugin_instance()
        {
            var pluginInstance = new AccountNumberPlugin();
            var pluginInstance2 = new AccountNumberPlugin();
            var stepId = Guid.NewGuid();
            _repository.SetPluginInstance(stepId, pluginInstance);
            _repository.SetPluginInstance(stepId, pluginInstance2);
            Assert.Equal(pluginInstance2, _repository.GetPluginInstance(stepId));
        }
        
        [Fact]
        public void Should_return_null_if_plugin_step_has_no_instances()
        {
            var pluginInstance = new AccountNumberPlugin();
            var stepId = Guid.NewGuid();
            _repository.SetPluginInstance(stepId, pluginInstance);
            Assert.Null(_repository.GetPluginInstance(Guid.NewGuid()));
        }
    }
}