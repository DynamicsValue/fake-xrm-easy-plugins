using System;

namespace FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions
{
    /// <summary>
    /// Exception raised when EntityTypeCode is used to register a plugin step but no proxy types assemblies were enabled
    /// </summary>
    public class RegisterEntityTypeCodePluginStepWithoutProxyTypesAssemblyException: Exception
    {
        /// <summary>
        /// Default constructor with the offending EntityTypeCode
        /// </summary>
        /// <param name="entityTypeCode"></param>
        public RegisterEntityTypeCodePluginStepWithoutProxyTypesAssemblyException(int entityTypeCode)
            : base(
                $"When using EntityTypeCode '{entityTypeCode.ToString()}' to register plugin steps, you need to enable at least one proxy types assembly first (context.EnableProxyTypes(Assembly)")
        {
            
        }
    }
}