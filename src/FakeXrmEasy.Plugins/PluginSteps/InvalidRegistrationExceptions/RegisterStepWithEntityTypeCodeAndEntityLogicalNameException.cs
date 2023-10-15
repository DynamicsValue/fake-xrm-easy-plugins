using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception raised when both an EntityTypeCode and EntityLogicalName were used to register a plugin step
    /// </summary>
    public class RegisterStepWithEntityTypeCodeAndEntityLogicalNameException: Exception
    {
        /// <summary>
        /// Default constructor with the offending EntityTypeCode and EntityLogicalName
        /// </summary>
        /// <param name="entityTypeCode"></param>
        /// <param name="logicalName"></param>
        public RegisterStepWithEntityTypeCodeAndEntityLogicalNameException(int entityTypeCode, string logicalName)
            : base($"Both EntityTypeCode '{entityTypeCode.ToString()}' and EntityLogicalName '{logicalName}' were used to register a plugin step. Please choose one or the other")
        {
            
        } 
    }
}