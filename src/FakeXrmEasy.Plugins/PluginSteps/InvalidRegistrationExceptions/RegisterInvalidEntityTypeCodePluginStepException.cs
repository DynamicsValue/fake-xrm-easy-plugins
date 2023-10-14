using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception raised when an EntityTypeCode was used to register a plugin step but the associated early-bound type was not found
    /// </summary>
    public class RegisterInvalidEntityTypeCodePluginStepException: Exception
    {
        /// <summary>
        /// Default constructor with the offending EntityTypeCode
        /// </summary>
        /// <param name="entityTypeCode"></param>
        public RegisterInvalidEntityTypeCodePluginStepException(int entityTypeCode)
            : base($"EntityTypeCode '{entityTypeCode.ToString()}' was not found in ProxyTypesAssemblies. Please check if the early bound type was generated.")
        {
            
        } 
    }
}