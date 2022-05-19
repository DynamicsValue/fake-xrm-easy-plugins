using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception raised when an attemp to register a plugin step was made with an invalid combination of message, entity name, stage and mode
    /// </summary>
    public class InvalidPluginStepRegistrationException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidPluginStepRegistrationException() : base($"Invalid Plugin Step Registration.")
        {

        }
    }
}
