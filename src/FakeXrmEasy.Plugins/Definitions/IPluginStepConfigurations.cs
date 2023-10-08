using System;

namespace FakeXrmEasy.Plugins.Definitions
{
    /// <summary>
    /// Allows setting secure and unsecure configurations as part of a plugin registration
    /// </summary>
    public interface IPluginStepConfigurations
    {
        /// <summary>
        /// The secure configuration string
        /// </summary>
        string SecureConfig { get; set; }
        
        /// <summary>
        /// The unsecure configuration string
        /// </summary>
        string UnsecureConfig { get; set; }
        
        /// <summary>
        /// Optional. The identifier of the secure configuration
        /// </summary>
        Guid SecureConfigId { get; set; }
    }
}