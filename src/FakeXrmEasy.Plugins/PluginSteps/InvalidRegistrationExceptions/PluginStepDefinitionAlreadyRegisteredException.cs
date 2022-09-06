using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception that is thrown when a plugin step with the same id is registered twice
    /// </summary>
    public class PluginStepDefinitionAlreadyRegisteredException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pluginStepId"></param>
        public PluginStepDefinitionAlreadyRegisteredException(Guid pluginStepId) : base($"A plugin step was already registered with the same Id='{pluginStepId}'")
        {

        }
    }
}
