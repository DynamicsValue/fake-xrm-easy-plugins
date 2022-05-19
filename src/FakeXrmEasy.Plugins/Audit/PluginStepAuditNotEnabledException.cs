using System;

namespace FakeXrmEasy.Plugins.Audit
{
    /// <summary>
    /// Exception thrown when querying plugin step audit without enabling them in the pipeline first
    /// </summary>
    public class PluginStepAuditNotEnabledException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginStepAuditNotEnabledException() : base("Audit must be enabled in the pipeline simulation options (PipelineOptions) before it can be used") 
        {
        }
    }
}
