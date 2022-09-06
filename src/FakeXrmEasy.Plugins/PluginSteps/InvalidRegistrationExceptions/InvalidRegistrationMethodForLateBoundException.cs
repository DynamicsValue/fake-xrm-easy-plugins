using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception thrown when a plugin step registration method for early bound entities was used to register late bound entities
    /// </summary>
    public class InvalidRegistrationMethodForLateBoundException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidRegistrationMethodForLateBoundException() 
            : base("This registration method requires EarlyBound entities and an EntityTypeCode property. If you want to register a plugin against an late - bound entity or an entity that doesn't have an EntityTypeCode attribute, please use the RegisterPluginStep<IPlugin> method that takes an entityLogicalName parameter instead.")
        {

        }
    }
}
