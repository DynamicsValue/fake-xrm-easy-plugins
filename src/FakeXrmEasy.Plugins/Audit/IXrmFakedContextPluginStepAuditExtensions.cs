using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware.Pipeline;

namespace FakeXrmEasy.Plugins.Audit
{
    /// <summary>
    /// IXrmFakedContext plugin step audit extensions
    /// </summary>
    public static class IXrmFakedContextPluginStepAuditExtensions
    {
        /// <summary>
        /// Returns an audit of all the plugin steps that were executed as part of pipeline simulation
        /// </summary>
        /// <param name="context">The context to where retrieve a plugin step audit from</param>
        /// <returns></returns>
        public static IPluginStepAudit GetPluginStepAudit(this IXrmFakedContext context)
        {
            var pipelineOptions = context.GetProperty<PipelineOptions>();
            if(!pipelineOptions.UsePluginStepAudit)
            {
                throw new PluginStepAuditNotEnabledException();
            }

            return context.GetProperty<IPluginStepAudit>();
        }
    }
}
