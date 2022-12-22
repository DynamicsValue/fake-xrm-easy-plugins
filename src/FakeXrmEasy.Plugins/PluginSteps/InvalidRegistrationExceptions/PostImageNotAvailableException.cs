using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception thrown when an invalid post image was attempted to be registered
    /// </summary>
    public class PostImageNotAvailableException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        public PostImageNotAvailableException(string name): 
            base($"The PostImage with name '{name}' is not available for the message and stage specified.")
        {

        }
    }
}
