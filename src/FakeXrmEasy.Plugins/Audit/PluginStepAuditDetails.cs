using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using System;
using FakeXrmEasy.Plugins.Definitions;

namespace FakeXrmEasy.Plugins.Audit
{
    /// <summary>
    /// Stores information about a given plugin step execution
    /// </summary>
    public class PluginStepAuditDetails
    {
        /// <summary>
        /// Timestamp of when this plugin step was executed, in UTC
        /// </summary>
        public DateTime ExecutedOn { get; set; }

        /// <summary>
        /// Id of the plugin step that was executed
        /// </summary>
        public Guid PluginStepId { get; set; }

        /// <summary>
        /// Stage that triggered this plugin execution
        /// </summary>
        public ProcessingStepStage Stage { get; set; }
        
        /// <summary>
        /// The mode the in which the plugin step was executed
        /// </summary>
        public ProcessingStepMode Mode { get; set; }

        /// <summary>
        /// Message that triggered this execution (i.e. Create, Update, and so on...)
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Target entity that triggered this plugin execution or null
        /// </summary>
        public Entity TargetEntity { get; set; }

        /// <summary>
        /// Target EntityReference that triggered this plugin execution or null
        /// </summary>
        public EntityReference TargetEntityReference { get; set; }

        /// <summary>
        /// Type of the plugin assembly that was triggered in this execution
        /// </summary>
        public Type PluginAssemblyType { get; set; }

        /// <summary>
        /// InputParameters that were part of the plugin execution
        /// </summary>
        public ParameterCollection InputParameters { get; set; }

        /// <summary>
        /// OutputParameters that were part of the plugin execution
        /// </summary>
        public ParameterCollection OutputParameters { get; set; }
        
        /// <summary>
        /// The plugin step definition that triggered the plugin execution
        /// </summary>
        public IPluginStepDefinition PluginStepDefinition { get; set; }
        
        /// <summary>
        /// The plugin execution context that was passed into the plugin when it was executed to check any other necessary properties
        /// </summary>
        public IPluginExecutionContext PluginContext { get; set; }
    }
}
