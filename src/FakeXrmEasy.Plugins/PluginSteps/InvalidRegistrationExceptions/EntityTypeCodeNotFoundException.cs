using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception thrown when using a plugin registration method that requires the entity type code to be present
    /// </summary>
    public class EntityTypeCodeNotFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="entityLogicalName"></param>
        public EntityTypeCodeNotFoundException(string entityLogicalName) : base($"This registration method requires an EntityTypeCode property. If you want to register a plugin against an entity that doesn't have an EntityTypeCode attribute, please use the RegisterPluginStep<IPlugin> method that takes an entityLogicalName parameter instead.")
        {

        }
    }
}
