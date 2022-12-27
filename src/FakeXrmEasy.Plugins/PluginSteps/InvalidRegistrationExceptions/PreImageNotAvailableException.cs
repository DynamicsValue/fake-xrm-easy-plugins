using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception thrown when an invalid preimage was attempted to be registered
    /// </summary>
    public class PreImageNotAvailableException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        public PreImageNotAvailableException(string name): 
            base($"The preimage with name '{name}' is not available for the message and stage specified.")
        {

        }
    }
}
