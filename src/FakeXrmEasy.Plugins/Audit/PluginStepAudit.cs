using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Plugins.Audit
{
    /// <summary>
    /// Interface to interact with auditing of plugin steps when using pipeline simulation
    /// </summary>
    public interface IPluginStepAudit
    {
        /// <summary>
        /// Creates a query against all the plugin steps that were executed by the pipeline simulation
        /// </summary>
        /// <returns></returns>
        IQueryable<PluginStepAuditDetails> CreateQuery();
    }

    /// <summary>
    /// Stores and interacts an audit of all the plugin steps executed when pipeline simulation is enabled
    /// </summary>
    public class PluginStepAudit : IPluginStepAudit
    {
        private readonly List<PluginStepAuditDetails> _audit;

        /// <summary>
        /// Default constructor. Creates an empty audit of plugin steps.
        /// </summary>
        public PluginStepAudit()
        {
            _audit = new List<PluginStepAuditDetails>();
        }

        /// <summary>
        /// Creates a query against all the plugin steps that were executed by the pipeline simulation
        /// </summary>
        /// <returns></returns>
        public IQueryable<PluginStepAuditDetails> CreateQuery()
        {
            return _audit.AsQueryable();
        }

        /// <summary>
        /// Internal. Adds a new plugin step audit details to the current PluginStepAudit
        /// </summary>
        /// <param name="details"></param>
        internal void Add(PluginStepAuditDetails details)
        {
            details.ExecutedOn = DateTime.UtcNow;
            _audit.Add(details);
        }
    }
}
