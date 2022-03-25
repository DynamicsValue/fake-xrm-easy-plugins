

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
        /// Default constructor with Pipeline Simulation enabled by default
        /// </summary>
        public PipelineOptions()
        {
            UsePipelineSimulation = true;
            UsePluginStepAudit = false;
        }
    }
}