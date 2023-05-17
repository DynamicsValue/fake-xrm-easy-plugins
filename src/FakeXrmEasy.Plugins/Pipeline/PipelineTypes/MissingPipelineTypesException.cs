using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using System;

namespace FakeXrmEasy.Plugins.Pipeline.PipelineTypes
{
    /// <summary>
    /// Exception thrown when using early bound types and not all the necessary pipeline types have been generated
    /// </summary>
    public class MissingPipelineTypesException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MissingPipelineTypesException() 
            : base($@"Some types needed for pipeline simulation but some are missing.
                        Please either generate all of them or do not generate any.
                        Types needed for pipeline simulation are:
                            '{PluginStepRegistrationEntityNames.PluginType}', 
                            '{PluginStepRegistrationEntityNames.SdkMessage}',
                            '{PluginStepRegistrationEntityNames.SdkMessageFilter}' and
                            '{PluginStepRegistrationEntityNames.SdkMessageProcessingStep}'.")
        {

        }
    }
}
