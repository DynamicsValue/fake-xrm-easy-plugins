using System;
using FakeXrmEasy.Plugins.Definitions;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// Stores information about a plugin step's secure and unsecure configurations
    /// </summary>
    public class PluginStepConfigurations : IPluginStepConfigurations
    {
        /// <summary>
        /// Secure configuration of the plugin step
        /// </summary>
        public string SecureConfig { get; set; }
        
        /// <summary>
        /// Unsecure configuration of the plugin step
        /// </summary>
        public string UnsecureConfig { get; set; }
        
        /// <summary>
        /// Optional. The identifier of the secure configuration
        /// </summary>
        public Guid SecureConfigId { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginStepConfigurations()
        {
            SecureConfigId = Guid.NewGuid();
        }
    }
}