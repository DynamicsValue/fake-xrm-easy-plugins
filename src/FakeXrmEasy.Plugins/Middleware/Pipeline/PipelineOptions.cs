

using System.Collections.Generic;
using System.Reflection;

namespace FakeXrmEasy.Middleware.Pipeline
{
    /// <summary>
    /// Configures Pipeline Simulation Options
    /// </summary>
    public class PipelineOptions
    {
        /// <summary>
        /// Enables Pipeline Simulation in middleware
        /// </summary>
        public bool UsePipelineSimulation { get; set; }

        /// <summary>
        /// Enables auditing of plugin steps that were executed in the pipeline
        /// </summary>
        public bool UsePluginStepAudit { get; set; }

        /// <summary>
        /// Enables vaidation of the registration of unsupported plugin steps
        /// </summary>
        public bool UsePluginStepRegistrationValidation { get; set; }

        /// <summary>
        /// Enables automatic discovery and registration of plugin steps
        /// </summary>
        public bool UseAutomaticPluginStepRegistration { get; set; }

        /// <summary>
        /// When UseAutomaticPluginStepRegistration is enabled: the assemblies where to look for plugin steps
        /// </summary>
        public IEnumerable<Assembly> PluginAssemblies { get; set; }

        /// <summary>
        /// Default constructor with Pipeline Simulation enabled by default
        /// </summary>
        public PipelineOptions()
        {
            UsePipelineSimulation = true;
            UsePluginStepAudit = false;
            UsePluginStepRegistrationValidation = true;
            UseAutomaticPluginStepRegistration = false;
            PluginAssemblies = null;
        }
    }
}