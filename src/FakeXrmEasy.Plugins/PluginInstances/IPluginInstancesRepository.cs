using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginInstances
{
    /// <summary>
    /// Contains a repository of plugin instances to execute per sdkmessageprocessingstep.
    /// Pipeline simulation will use any of these instances over any other default parameterless constructor or constructor with configurations
    /// This repository allows advanced scenarios of using user-defined plugin instances with custom constructors (for DI purposes)
    /// </summary>
    internal interface IPluginInstancesRepository
    {
        /// <summary>
        /// Returns a plugin instance for the associated SdkMessageProcessingStep, if any was previously set
        /// </summary>
        /// <param name="sdkMessageProcessingStepId">The Id of the SdkMessageProcessingStep</param>
        /// <returns>The plugin instance or null otherwise</returns>
        IPlugin GetPluginInstance(Guid sdkMessageProcessingStepId);
        
        /// <summary>
        /// Stores a new plugin instance for the associated SdkMessageProcessingStep
        /// </summary>
        /// <param name="sdkMessageProcessingStepId">The Id of the SdkMessageProcessingStep</param>
        /// <param name="pluginInstance">The plugin instance to store and that will be used in pipeline simulation</param>
        void SetPluginInstance(Guid sdkMessageProcessingStepId, IPlugin pluginInstance);
    }
}