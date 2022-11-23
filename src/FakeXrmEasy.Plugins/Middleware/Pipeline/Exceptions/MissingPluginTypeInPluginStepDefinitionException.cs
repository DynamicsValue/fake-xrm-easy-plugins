using System;

namespace FakeXrmEasy.Plugins.Middleware.Pipeline.Exceptions
{
    /// <summary>
    /// Exception thrown when a plugin step definition used in a CustomRegistrationFunc is null
    /// </summary>
    public class MissingPluginTypeInPluginStepDefinitionException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MissingPluginTypeInPluginStepDefinitionException(): base("PluginType property in the plugin step definition is null or empty, please you must set it to the FullName of the plugin type (i.e. typeof(FollowupPlugin).FullName)")
        {

        }
    }
}
