using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception raised when an attemp to register a plugin step was made with an invalid primary entity name
    /// </summary>
    public class InvalidPrimaryEntityNameException : Exception
    {
        /// <summary>
        /// Default constructor that requires the invalid primary entity name
        /// </summary>
        /// <param name="entityLogicalName"></param>
        public InvalidPrimaryEntityNameException(string entityLogicalName) : base($"Invalid Primary Entity Name specified '{entityLogicalName}'.")
        {

        }
    }
}
