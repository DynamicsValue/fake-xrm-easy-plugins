using FakeXrmEasy.Abstractions.Plugins.Enums;
using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// Contains the properties of a plugin step that are needed to execute it
    /// </summary>
    public class PluginStepDefinition
    {
        /// <summary>
        /// Primary key of the stored sdkmessageprocessingstep
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Stage where this plugins runs.
        /// </summary>
        public ProcessingStepStage Stage { get; set; }

        /// <summary>
        /// Name of the OrganizationRequest that triggers this plugin step
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Mode of this plugin step
        /// </summary>
        public ProcessingStepMode Mode { get; set; }
        
        /// <summary>
        /// If present, the plugin will only execute for entities whose EntityTypeCode matches this value, or any entity otherwise
        /// </summary>
        public int? EntityTypeCode { get; set; }

        /// <summary>
        /// The order in which this plugin will run relative to other plugin steps of the same stage and mode
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// The attributes used to filter the execution of this plugin. The plugin will only execute if any of the attributes are present in the request,
        /// or will run regardless of any attributes if empty
        /// </summary>
        public List<string> FilteringAttributes { get; set; }

        /// <summary>
        /// The name of the assembly where the plugin type will be searched and executed
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Type of the plugin to be executed (the plugin class name)
        /// </summary>
        public string PluginType { get; set; }

    }
}
