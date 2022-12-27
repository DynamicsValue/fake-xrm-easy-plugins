using System;

namespace FakeXrmEasy.Plugins.Middleware.Pipeline.Exceptions
{
    /// <summary>
    /// Exception raised when automatic registration of plugin steps is enabled but the discovery function is not defined
    /// </summary>
    public class CustomDiscoveryFunctionMissingException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomDiscoveryFunctionMissingException()
            :base("When using the automatic registration of plugin steps (UseAutomaticPluginStepRegistration = true), a custom discovery function is needed (CustomPluginStepDiscoveryFunction).")
        {

        }
    }
}
