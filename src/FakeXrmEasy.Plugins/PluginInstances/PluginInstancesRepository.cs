using System;
using System.Collections.Concurrent;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginInstances
{
    /// <summary>
    /// The implementation of IPluginInstancesRepository
    /// </summary>
    internal class PluginInstancesRepository : IPluginInstancesRepository
    {
        private readonly ConcurrentDictionary<Guid, IPlugin> _repository;

        public PluginInstancesRepository()
        {
            _repository = new ConcurrentDictionary<Guid, IPlugin>();
        }
        
        public IPlugin GetPluginInstance(Guid sdkMessageProcessingStepId)
        {
            return _repository.ContainsKey(sdkMessageProcessingStepId) ? _repository[sdkMessageProcessingStepId] : null;
        }

        public void SetPluginInstance(Guid sdkMessageProcessingStepId, IPlugin pluginInstance)
        {
            _repository.AddOrUpdate(sdkMessageProcessingStepId,
                (guid) => pluginInstance,
                (guid, oldPluginInstance) => pluginInstance);
        }
    }
}